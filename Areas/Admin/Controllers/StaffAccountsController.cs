using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PopPop.Models;
using PopPop.Services.Interfaces;

namespace PopPop.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public sealed class StaffAccountsController : Controller
{
    /// <summary>TempData key of a password shown once to the admin (initial or temporary).</summary>
    public const string IssuedPasswordKey = "IssuedPassword";

    private readonly IStaffAccountService _staffAccountService;

    public StaffAccountsController(IStaffAccountService staffAccountService)
    {
        _staffAccountService = staffAccountService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search, string? status, CancellationToken cancellationToken)
    {
        var model = await _staffAccountService.GetListAsync(search, status, cancellationToken);
        return View(model);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreateStaffAccountViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateStaffAccountViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var password = await _staffAccountService.CreateAsync(model, cancellationToken);
            TempData["SuccessMessage"] = $"Đã tạo tài khoản nhân viên {model.FullName.Trim()}.";
            TempData[IssuedPasswordKey] = password;
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id, CancellationToken cancellationToken)
    {
        var model = await _staffAccountService.GetEditAsync(id, cancellationToken);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, EditStaffAccountViewModel model, CancellationToken cancellationToken)
    {
        model.Id = id;
        if (ModelState.IsValid)
        {
            try
            {
                await _staffAccountService.UpdateAsync(model, cancellationToken);
                TempData["SuccessMessage"] = "Đã cập nhật thông tin nhân viên.";
                return RedirectToAction(nameof(Edit), new { id });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException exception)
            {
                ModelState.AddModelError(string.Empty, exception.Message);
            }
        }

        // Re-display read-only account facts next to the posted values.
        var current = await _staffAccountService.GetEditAsync(id, cancellationToken);
        if (current is null)
        {
            return NotFound();
        }

        model.MustChangePassword = current.MustChangePassword;
        model.IsLocked = current.IsLocked;
        model.LockReason = current.LockReason;
        model.CreatedAt = current.CreatedAt;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(string id, CancellationToken cancellationToken)
    {
        try
        {
            TempData[IssuedPasswordKey] = await _staffAccountService.ResetPasswordAsync(id, cancellationToken);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Lock(string id, string? reason, CancellationToken cancellationToken) =>
        RunAsync(id, () => _staffAccountService.LockAsync(id, reason ?? string.Empty, cancellationToken),
            "Đã khóa tài khoản. Nhân viên sẽ bị đăng xuất và không thể đăng nhập khu vực quản lý.");

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Unlock(string id, CancellationToken cancellationToken) =>
        RunAsync(id, () => _staffAccountService.UnlockAsync(id, cancellationToken), "Đã mở khóa tài khoản.");

    private async Task<IActionResult> RunAsync(string id, Func<Task> action, string successMessage)
    {
        try
        {
            await action();
            TempData["SuccessMessage"] = successMessage;
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Edit), new { id });
    }
}
