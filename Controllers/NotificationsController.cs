using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PopPop.Services.Interfaces;

namespace PopPop.Controllers;

[Authorize]
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
        return View(await _notificationService.GetPageAsync(UserId, page, cancellationToken));
    }

    /// <summary>Marks the notification as read, then opens what it points to.</summary>
    [HttpGet]
    public async Task<IActionResult> Open(int id, CancellationToken cancellationToken)
    {
        var url = await _notificationService.OpenAsync(id, UserId, cancellationToken);
        if (url is null)
        {
            return NotFound();
        }

        // Stored links are site-relative; LocalRedirect refuses anything pointing to another site.
        return Url.IsLocalUrl(url) ? LocalRedirect(url) : RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllRead(CancellationToken cancellationToken)
    {
        await _notificationService.MarkAllReadAsync(UserId, cancellationToken);
        var referer = Request.Headers.Referer.ToString();
        var back = Uri.TryCreate(referer, UriKind.Absolute, out var uri) && uri.Host == Request.Host.Host ? uri.PathAndQuery : null;
        return back is not null && Url.IsLocalUrl(back) ? LocalRedirect(back) : RedirectToAction(nameof(Index));
    }

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("Không xác định được tài khoản hiện tại.");
}
