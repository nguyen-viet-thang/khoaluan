using PopPop.Data.Entities.Enums;
using PopPop.Models;

namespace PopPop.Services.Interfaces;

/// <summary>
/// Notifications: stored in-site (header bell) and pushed in real time over SignalR. The shop does not use email.
/// Delivery problems (no live connection) are logged and never fail the calling operation.
/// </summary>
public interface INotificationService
{
    /// <summary>Notifies the customer that staff moved their claim to a new status.</summary>
    Task NotifyClaimStatusChangedAsync(
        int claimId,
        string claimCode,
        string customerUserId,
        ClaimStatus newStatus,
        ClaimResolution? resolution,
        string? staffNote,
        CancellationToken cancellationToken = default);

    /// <summary>Staff sent the customer a message about a claim without changing its status.</summary>
    Task NotifyClaimMessageAsync(int claimId, string claimCode, string customerUserId, string message, CancellationToken cancellationToken = default);

    /// <summary>Notifies the customer that staff moved their order to a new status.</summary>
    Task NotifyOrderStatusChangedAsync(
        int orderId,
        string orderCode,
        string customerUserId,
        OrderStatus newStatus,
        decimal totalAmount,
        string? cancelReason,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifies every active Admin/Staff account (in-site + real time).
    /// </summary>
    /// <param name="url">Site-relative link, e.g. /Staff/Claims/Details/5.</param>
    Task NotifyStaffAsync(string title, string? message, string url, CancellationToken cancellationToken = default);

    Task<NotificationListViewModel> GetPageAsync(string userId, int page, CancellationToken cancellationToken = default);

    /// <summary>Marks one notification as read and returns its link (null if it does not belong to the user).</summary>
    Task<string?> OpenAsync(int notificationId, string userId, CancellationToken cancellationToken = default);

    Task MarkAllReadAsync(string userId, CancellationToken cancellationToken = default);
}
