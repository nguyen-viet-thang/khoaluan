using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Globalization;
using PopPop.Data.Entities;
using PopPop.Data.Entities.Enums;
using PopPop.Models;
using PopPop.Services.Interfaces;

namespace PopPop.Controllers;

[Authorize]
public class OrdersController : Controller
{
    private readonly ICartService _cartService;
    private readonly IOrderService _orderService;
    private readonly UserManager<User> _userManager;
    private readonly IWarrantyClaimService _claimService;
    private readonly IVnPayPaymentService _vnPayPaymentService;

    public OrdersController(
        ICartService cartService,
        IOrderService orderService,
        UserManager<User> userManager,
        IWarrantyClaimService claimService,
        IVnPayPaymentService vnPayPaymentService)
    {
        _cartService = cartService;
        _orderService = orderService;
        _userManager = userManager;
        _claimService = claimService;
        _vnPayPaymentService = vnPayPaymentService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var orders = await _orderService.GetOrdersForUserAsync(GetUserId(), cancellationToken);
        return View(orders);
    }

    [HttpGet]
    public async Task<IActionResult> Checkout(CancellationToken cancellationToken)
    {
        var model = new CheckoutViewModel();
        await LoadCartSummaryAsync(model, cancellationToken);
        if (model.Items.Count == 0 || !model.Items.All(item => item.StockQuantity >= item.Quantity))
        {
            TempData["CartError"] = model.Items.Count == 0
                ? "Giỏ hàng đang trống."
                : "Tồn kho đã thay đổi. Vui lòng kiểm tra lại giỏ hàng trước khi đặt hàng.";
            return RedirectToAction("Index", "Cart");
        }

        // Save the customer retyping: reuse the last delivery details, or fall back to the account profile.
        if (!await _orderService.PrefillCheckoutFromLastOrderAsync(GetUserId(), model, cancellationToken))
        {
            var user = await _userManager.GetUserAsync(User);
            model.RecipientName = user?.FullName ?? string.Empty;
            model.RecipientPhone = user?.PhoneNumber ?? string.Empty;
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(CheckoutViewModel model, CancellationToken cancellationToken)
    {
        if (model.PaymentMethod == PaymentMethod.VnPay && !_vnPayPaymentService.IsConfigured)
        {
            ModelState.AddModelError(nameof(model.PaymentMethod), "Cổng VNPAY tạm thời không khả dụng, vui lòng chọn thanh toán khi nhận hàng.");
        }

        if (ModelState.IsValid)
        {
            try
            {
                var order = await _orderService.PlaceOrderAsync(GetUserId(), model, model.PaymentMethod, cancellationToken);
                if (model.PaymentMethod == PaymentMethod.VnPay)
                {
                    return RedirectToAction(nameof(Pay), new { id = order.OrderId });
                }

                TempData["OrderSuccess"] = "Đặt hàng thành công! Cửa hàng sẽ liên hệ xác nhận đơn trong thời gian sớm nhất.";
                return RedirectToAction(nameof(Details), new { id = order.OrderId });
            }
            catch (InvalidOperationException exception)
            {
                ModelState.AddModelError(string.Empty, exception.Message);
            }
        }

        await LoadCartSummaryAsync(model, cancellationToken);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Pay(int id, CancellationToken cancellationToken)
    {
        var order = await _orderService.GetDetailsForUserAsync(id, GetUserId(), cancellationToken);
        if (order is null)
        {
            return NotFound();
        }

        if (order.PaymentStatus == PaymentStatus.Paid)
        {
            TempData["OrderSuccess"] = "Đơn hàng đã được thanh toán.";
            return RedirectToAction(nameof(Details), new { id });
        }

        if (order.PaymentDeadline is not { } deadline || deadline <= DateTime.UtcNow)
        {
            TempData["OrderError"] = order.Status == OrderStatus.Cancelled
                ? "Đơn hàng đã bị hủy nên không thể thanh toán."
                : "Đơn hàng không còn chờ thanh toán VNPAY.";
            return RedirectToAction(nameof(Details), new { id });
        }

        if (!_vnPayPaymentService.IsConfigured)
        {
            TempData["OrderError"] = "Cổng VNPAY tạm thời không khả dụng. Bạn có thể chuyển sang thanh toán khi nhận hàng.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var returnUrl = Url.Action(nameof(VnPayReturn), "Orders", values: null, protocol: Request.Scheme)
            ?? throw new InvalidOperationException("Không thể tạo địa chỉ nhận kết quả VNPAY.");
        var paymentUrl = _vnPayPaymentService.CreatePaymentUrl(
            id, order.OrderCode, order.TotalAmount, GetClientIpAddress(), returnUrl, deadline);
        return Redirect(paymentUrl);
    }

    /// <summary>
    /// The customer's browser comes back from VNPAY. The signed result is applied right away (the same idempotent
    /// handler as the IPN), so the order shows "paid" immediately even when VNPAY cannot reach the IPN URL (e.g. localhost).
    /// </summary>
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> VnPayReturn(CancellationToken cancellationToken)
    {
        var callback = _vnPayPaymentService.ParseCallback(Request.Query);
        if (!callback.IsValid)
        {
            TempData["OrderError"] = "Không thể xác thực kết quả trả về từ VNPAY. Nếu tài khoản đã bị trừ tiền, vui lòng liên hệ cửa hàng.";
            return RedirectToAction(nameof(Index));
        }

        var resultCode = await _orderService.ProcessVnPayNotificationAsync(
            callback.OrderId, callback.Amount, callback.Succeeded, callback.TransactionNo, cancellationToken);

        if (callback.Succeeded && resultCode is "00" or "02")
        {
            TempData["OrderSuccess"] = "Thanh toán VNPAY thành công! Cửa hàng sẽ xác nhận và chuẩn bị đơn hàng cho bạn.";
        }
        else if (resultCode == "04")
        {
            TempData["OrderError"] = "Số tiền thanh toán không khớp với đơn hàng. Vui lòng liên hệ cửa hàng để được hỗ trợ.";
        }
        else
        {
            TempData["OrderError"] = $"Thanh toán chưa hoàn tất: {_vnPayPaymentService.DescribeResponseCode(callback.ResponseCode)} "
                + "Bạn có thể thanh toán lại hoặc chuyển sang thanh toán khi nhận hàng.";
        }

        return RedirectToAction(nameof(Details), new { id = callback.OrderId });
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> VnPayIpn(CancellationToken cancellationToken)
    {
        if (Request.Query.Count == 0)
        {
            return VnPayIpnResponse("99");
        }

        var callback = _vnPayPaymentService.ParseCallback(Request.Query);
        if (!callback.IsValid)
        {
            return VnPayIpnResponse(callback.ResponseCode);
        }

        var resultCode = await _orderService.ProcessVnPayNotificationAsync(
            callback.OrderId, callback.Amount, callback.Succeeded, callback.TransactionNo, cancellationToken);
        return VnPayIpnResponse(resultCode);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SwitchToCod(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _orderService.SwitchToCashOnDeliveryAsync(id, GetUserId(), cancellationToken);
            TempData["OrderSuccess"] = "Đã chuyển sang thanh toán khi nhận hàng (COD). Cửa hàng sẽ liên hệ xác nhận đơn sớm nhất.";
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException exception)
        {
            TempData["OrderError"] = exception.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var order = await _orderService.GetDetailsForUserAsync(id, GetUserId(), cancellationToken);
        if (order is null)
        {
            return NotFound();
        }

        order.Claims = await _claimService.GetForCustomerAsync(GetUserId(), id, cancellationToken);
        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, string? reason, CancellationToken cancellationToken)
    {
        try
        {
            await _orderService.CancelByCustomerAsync(id, GetUserId(), reason, cancellationToken);
            TempData["OrderSuccess"] = "Đã hủy đơn hàng.";
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException exception)
        {
            TempData["OrderError"] = exception.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task LoadCartSummaryAsync(CheckoutViewModel model, CancellationToken cancellationToken)
    {
        var cart = await _cartService.GetCartAsync(GetUserId(), cancellationToken);
        model.Items = cart.Items;
        model.ShippingFee = _orderService.CalculateShippingFee(model.Subtotal);
        model.FreeShippingThreshold = _orderService.ShippingPolicy.FreeShippingThreshold;
        model.VnPayAvailable = _vnPayPaymentService.IsConfigured;
        model.VnPayTimeoutMinutes = (int)_vnPayPaymentService.PaymentTimeout.TotalMinutes;
    }

    /// <summary>IPv4 address for vnp_IpAddr (VNPAY rejects "::1" and IPv6-mapped forms).</summary>
    private string GetClientIpAddress()
    {
        var address = HttpContext.Connection.RemoteIpAddress;
        if (address is null || System.Net.IPAddress.IsLoopback(address))
        {
            return "127.0.0.1";
        }

        return address.IsIPv4MappedToIPv6 || address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork
            ? address.MapToIPv4().ToString()
            : address.ToString();
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Không xác định được tài khoản hiện tại.");
    }

    private ContentResult VnPayIpnResponse(string responseCode)
    {
        var message = responseCode switch
        {
            "00" => "Confirm Success",
            "01" => "Order not found",
            "02" => "Order already confirmed",
            "04" => "Invalid amount",
            "97" => "Invalid signature",
            _ => "Invalid request"
        };
        var json = JsonSerializer.Serialize(new { RspCode = responseCode, Message = message });
        return Content(json, "application/json");
    }
}
