using PopPop.Data.Entities;

namespace PopPop.Repositories.Interfaces;

public interface INotificationRepository : IRepository<Notification, int>
{
    Task<IReadOnlyList<Notification>> GetForUserAsync(string userId, int skip, int take, CancellationToken cancellationToken = default);
    Task<int> CountForUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<int> CountUnreadAsync(string userId, CancellationToken cancellationToken = default);
    /// <summary>Marks every unread notification of the user as read; returns how many were updated.</summary>
    Task<int> MarkAllReadAsync(string userId, DateTime readAt, CancellationToken cancellationToken = default);

    /// <summary>Ids of Admin/Staff accounts that are not locked (recipients of back-office notifications).</summary>
    Task<IReadOnlyList<string>> GetActiveStaffUserIdsAsync(CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<Notification> notifications, CancellationToken cancellationToken = default);
}
