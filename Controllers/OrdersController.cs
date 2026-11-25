using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PopPop.Data.Entities;
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

    public OrdersController(
        ICartService cartService,
        IOrderService orderService,
        UserManager<User> userManager,
        IWarrantyClaimService claimService)
    {
        _cartService = cartService;
        _orderService = orderService;
        _userManager = userManager;
        _claimService = claimService;
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
                var orderId = await _orderService.PlaceOrderAsync(GetUserId(), model, cancellationToken);
                TempData["OrderSuccess"] = "Đặt hàng thành công! Cửa hàng sẽ liên hệ xác nhận đơn trong thời gian sớm nhất.";
                return RedirectToAction(nameof(Details), new { id = orderId });
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
}
