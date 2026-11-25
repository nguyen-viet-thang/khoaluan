using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PopPop.Services.Interfaces;

namespace PopPop.Areas.Staff.Controllers;

/// <summary>Full notification list for back-office users (opening and marking read reuse the shared /Notifications actions).</summary>
[Area("Staff")]
[Authorize(Roles = "Admin,Staff")]
public sealed class NotificationsController : Controller
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int page = 1, CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return View(await _notificationService.GetPageAsync(userId, page, cancellationToken));
    }
}
