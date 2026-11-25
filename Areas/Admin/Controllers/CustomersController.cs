using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PopPop.Services.Interfaces;

namespace PopPop.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public sealed class CustomersController : Controller
{
    private readonly ICustomerManagementService _customerService;

    public CustomersController(ICustomerManagementService customerService)
    {
        _customerService = customerService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        string? search,
        string? status,
        string? sort,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        var model = await _customerService.GetListAsync(search, status, sort, page, cancellationToken);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Details(string id, CancellationToken cancellationToken)
    {
        var model = await _customerService.GetDetailsAsync(id, cancellationToken);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Lock(string id, string? reason, CancellationToken cancellationToken)
    {
        try
        {
            await _customerService.LockAsync(id, reason ?? string.Empty, cancellationToken);
            TempData["SuccessMessage"] = "Đã khóa tài khoản. Khách hàng sẽ bị đăng xuất và không thể đăng nhập lại.";
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unlock(string id, CancellationToken cancellationToken)
    {
        try
        {
            await _customerService.UnlockAsync(id, cancellationToken);
            TempData["SuccessMessage"] = "Đã mở khóa tài khoản.";
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(string id, CancellationToken cancellationToken)
    {
        try
        {
            var temporary = await _customerService.ResetPasswordAsync(id, cancellationToken);
            // Shown once; the customer must replace it at the next sign-in.
            TempData["TemporaryPassword"] = temporary;
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id, string? confirmPhone, CancellationToken cancellationToken)
    {
        try
        {
            var phone = await _customerService.DeleteAsync(id, confirmPhone ?? string.Empty, cancellationToken);
            TempData["SuccessMessage"] = $"Đã xóa vĩnh viễn tài khoản {phone}.";
            return RedirectToAction(nameof(Index));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
