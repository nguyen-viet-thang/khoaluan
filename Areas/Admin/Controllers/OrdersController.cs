using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PopPop.Data.Entities.Enums;
using PopPop.Services.Interfaces;

namespace PopPop.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public sealed class OrdersController : Controller
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(OrderStatus? status, string? search, CancellationToken cancellationToken)
    {
        var model = await _orderService.GetManagementOrdersAsync(status, search, cancellationToken);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var model = await _orderService.GetManagementOrderDetailsAsync(id, cancellationToken);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, OrderStatus nextStatus, string? cancelReason, CancellationToken cancellationToken)
    {
        try
        {
            await _orderService.UpdateOrderStatusAsync(id, nextStatus, cancelReason, User.Identity?.Name ?? "nhân viên", cancellationToken);
            var updated = await _orderService.GetManagementOrderDetailsAsync(id, cancellationToken);
            TempData["SuccessMessage"] = updated is null
                ? "Đã cập nhật trạng thái đơn hàng."
                : $"Đã chuyển đơn {updated.OrderCode} sang \"{updated.StatusText}\".";
            // Back to the work queue: the order now sits in its new status group.
            return RedirectToAction(nameof(Index));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }
}
