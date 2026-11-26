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

        if (order.PaymentMethod != PaymentMethod.VnPay || order.Status == OrderStatus.Cancelled)
        {
            return BadRequest("Đơn hàng không thể thanh toán qua VNPAY.");
        }

        if (order.PaymentStatus == PaymentStatus.Paid)
        {
            return RedirectToAction(nameof(Details), new { id });
        }

        var returnUrl = Url.Action(nameof(VnPayReturn), "Orders", values: null, protocol: Request.Scheme)
            ?? throw new InvalidOperationException("Không thể tạo địa chỉ nhận kết quả VNPAY.");
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "127.0.0.1";
        var paymentUrl = _vnPayPaymentService.CreatePaymentUrl(
            id.ToString(CultureInfo.InvariantCulture), order.TotalAmount, ipAddress, returnUrl);
        return Redirect(paymentUrl);
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult VnPayReturn()
    {
        if (!_vnPayPaymentService.ValidateSignature(Request.Query)
            || !Request.Query.TryGetValue("vnp_TxnRef", out var reference)
            || reference.Count != 1
            || !int.TryParse(reference[0], NumberStyles.None, CultureInfo.InvariantCulture, out var orderId))
        {
            TempData["OrderError"] = "Không thể xác thực kết quả trả về từ VNPAY.";
            return RedirectToAction("Index", "Home");
        }

        var succeeded = Request.Query["vnp_ResponseCode"] == "00"
            && Request.Query["vnp_TransactionStatus"] == "00";
        TempData[succeeded ? "OrderSuccess" : "OrderError"] = succeeded
            ? "VNPAY đã trả kết quả. Trạng thái đơn hàng sẽ được cập nhật sau khi hệ thống xác nhận giao dịch."
            : "Thanh toán VNPAY chưa hoàn tất. Bạn có thể thử thanh toán lại từ chi tiết đơn hàng.";
        return RedirectToAction(nameof(Details), new { id = orderId });
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> VnPayIpn(CancellationToken cancellationToken)
    {
        var query = Request.Query;
        if (query.Count == 0)
        {
            return VnPayIpnResponse("99");
        }

        if (!_vnPayPaymentService.ValidateSignature(query))
        {
            return VnPayIpnResponse("97");
        }

        if (!query.TryGetValue("vnp_TxnRef", out var reference)
            || reference.Count != 1
            || !int.TryParse(reference[0], NumberStyles.None, CultureInfo.InvariantCulture, out var orderId)
            || !query.TryGetValue("vnp_ResponseCode", out var responseCode)
            || responseCode.Count != 1
            || !query.TryGetValue("vnp_TransactionStatus", out var transactionStatus)
            || transactionStatus.Count != 1)
        {
            return VnPayIpnResponse("99");
        }

        if (!query.TryGetValue("vnp_Amount", out var amountValue)
            || amountValue.Count != 1
            || !long.TryParse(amountValue[0], NumberStyles.None, CultureInfo.InvariantCulture, out var amount))
        {
            return VnPayIpnResponse("04");
        }

        var paymentSucceeded = responseCode[0] == "00" && transactionStatus[0] == "00";
        var resultCode = await _orderService.ProcessVnPayNotificationAsync(
            orderId, amount, paymentSucceeded, cancellationToken);
        return VnPayIpnResponse(resultCode);
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
