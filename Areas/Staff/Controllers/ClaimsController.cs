using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PopPop.Data.Entities;
using PopPop.Data.Entities.Enums;
using PopPop.Models;
using PopPop.Services.Interfaces;

namespace PopPop.Areas.Staff.Controllers;

/// <summary>Staff/admin processing of warranty and receipt-defect claims.</summary>
[Area("Staff")]
[Authorize(Roles = "Admin,Staff")]
public sealed class ClaimsController : Controller
{
    private readonly IWarrantyClaimService _claimService;
    private readonly UserManager<User> _userManager;

    public ClaimsController(IWarrantyClaimService claimService, UserManager<User> userManager)
    {
        _claimService = claimService;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index(ClaimStatus? status, ClaimType? type, string? search, CancellationToken cancellationToken)
    {
        var model = await _claimService.GetForStaffAsync(status, type, search, cancellationToken);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var model = await _claimService.GetDetailsForStaffAsync(id, cancellationToken);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Decide(int id, ClaimDecisionViewModel decision, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            var actorName = user?.FullName ?? user?.UserName ?? "Nhân viên";
            await _claimService.DecideAsync(id, User.FindFirstValue(ClaimTypes.NameIdentifier)!, actorName, decision, cancellationToken);
            var updated = await _claimService.GetDetailsForStaffAsync(id, cancellationToken);
            TempData["SuccessMessage"] = updated is null
                ? $"Đã chuyển yêu cầu sang \"{ClaimDisplay.StatusText(decision.NextStatus)}\"."
                : $"Đã chuyển yêu cầu {updated.Code} sang \"{ClaimDisplay.StatusText(decision.NextStatus)}\".";
            // Back to the work queue: the claim now sits in its new status group.
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveNote(int id, string? note, CancellationToken cancellationToken)
    {
        try
        {
            await _claimService.SaveStaffNoteAsync(id, note, cancellationToken);
            TempData["SuccessMessage"] = "Đã lưu ghi chú nội bộ.";
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendMessage(int id, string? message, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            var actorName = user?.FullName ?? user?.UserName ?? "Nhân viên";
            await _claimService.SendMessageAsync(id, User.FindFirstValue(ClaimTypes.NameIdentifier)!, actorName, message ?? string.Empty, cancellationToken);
            TempData["SuccessMessage"] = "Đã gửi lời nhắn cho khách hàng.";
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

    [HttpGet]
    public async Task<IActionResult> Media(int id, CancellationToken cancellationToken)
    {
        var file = await _claimService.GetMediaFileAsync(id, userId: null, cancellationToken);
        if (file is null)
        {
            return NotFound();
        }

        Response.Headers.XContentTypeOptions = "nosniff";
        return PhysicalFile(file.Value.Path, file.Value.ContentType, enableRangeProcessing: true);
    }
}
