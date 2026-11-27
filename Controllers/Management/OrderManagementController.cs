using Microsoft.AspNetCore.Mvc;
using PopPop.Data.Entities.Enums;
using PopPop.Services.Interfaces;

namespace PopPop.Controllers.Management;

/// <summary>
/// Order processing shared by the Admin and Staff areas (same rules, different area/route and authorization,
/// which the concrete controllers declare). Views: Views/Shared/_OrderManagementList and _OrderManagementDetails.
/// </summary>
public abstract class OrderManagementController : Controller
{
    private readonly IOrderService _orderService;

    protected OrderManagementController(IOrderService orderService)
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
    public async Task<IActionResult> Details(int id, string? returnUrl, CancellationToken cancellationToken)
    {
        var model = await _orderService.GetManagementOrderDetailsAsync(id, cancellationToken);
        if (model is null)
        {
            return NotFound();
        }

        ViewData["ReturnUrl"] = LocalOrNull(returnUrl);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(
        int id,
        OrderStatus nextStatus,
        string? cancelReason,
        string? returnUrl,
        bool fromDetails,
        CancellationToken cancellationToken)
    {
        try
        {
            await _orderService.UpdateOrderStatusAsync(id, nextStatus, cancelReason, ActorName, cancellationToken);
            var updated = await _orderService.GetManagementOrderDetailsAsync(id, cancellationToken);
            TempData["SuccessMessage"] = updated is null
                ? "Đã cập nhật trạng thái đơn hàng."
                : $"Đã chuyển đơn {updated.OrderCode} sang \"{updated.StatusText}\".";
            // Back to the work queue (keeping its filters): the order now sits in its new status group.
            return BackToList(returnUrl);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
        }

        // A failed quick action stays on the list; a failed action on the details page stays there.
        return fromDetails
            ? RedirectToAction(nameof(Details), new { id, returnUrl = LocalOrNull(returnUrl) })
            : BackToList(returnUrl);
    }

    /// <summary>Moves every selected order one step forward (bulk action on the list).</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Advance(int[] ids, string? returnUrl, CancellationToken cancellationToken)
    {
        if (ids.Length == 0)
        {
            TempData["ErrorMessage"] = "Hãy chọn ít nhất một đơn hàng.";
            return BackToList(returnUrl);
        }

        var result = await _orderService.AdvanceOrdersAsync(ids, ActorName, cancellationToken);
        if (result.Updated.Count > 0)
        {
            TempData["SuccessMessage"] = $"Đã chuyển {result.Updated.Count} đơn sang bước tiếp theo: {string.Join(", ", result.Updated)}.";
        }

        if (result.Failed.Count > 0)
        {
            TempData["ErrorMessage"] = $"{result.Failed.Count} đơn chưa thể chuyển: {string.Join("; ", result.Failed)}";
        }

        return BackToList(returnUrl);
    }

    private string ActorName => User.Identity?.Name ?? "nhân viên";

    private IActionResult BackToList(string? returnUrl) =>
        LocalOrNull(returnUrl) is { } url ? LocalRedirect(url) : RedirectToAction(nameof(Index));

    private string? LocalOrNull(string? url) =>
        !string.IsNullOrWhiteSpace(url) && Url.IsLocalUrl(url) ? url : null;
}
