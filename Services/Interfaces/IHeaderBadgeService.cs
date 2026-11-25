using PopPop.Models;

namespace PopPop.Services.Interfaces;

public interface IHeaderBadgeService
{
    /// <summary>Cart size, unread shop messages, unread notifications and the latest notifications for the header bell.</summary>
    Task<HeaderBadges> GetCountsAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>Unread notifications, the latest ones and the number of claims waiting for review (back office).</summary>
    Task<StaffBadges> GetStaffBadgesAsync(string userId, CancellationToken cancellationToken = default);
}
