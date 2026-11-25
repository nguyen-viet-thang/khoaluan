using Microsoft.EntityFrameworkCore;
using PopPop.Data;
using PopPop.Data.Entities;
using PopPop.Repositories.Interfaces;

namespace PopPop.Repositories.Implementations;

public sealed class NotificationRepository : Repository<Notification, int>, INotificationRepository
{
    public NotificationRepository(PopPopDbContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<Notification>> GetForUserAsync(string userId, int skip, int take, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking()
            .Where(notification => notification.UserId == userId)
            .OrderByDescending(notification => notification.CreatedAt)
            .ThenByDescending(notification => notification.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountForUserAsync(string userId, CancellationToken cancellationToken = default) =>
        DbSet.CountAsync(notification => notification.UserId == userId, cancellationToken);

    public Task<int> CountUnreadAsync(string userId, CancellationToken cancellationToken = default) =>
        DbSet.CountAsync(notification => notification.UserId == userId && !notification.IsRead, cancellationToken);

    public async Task<IReadOnlyList<string>> GetActiveStaffUserIdsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        string[] backOfficeRoles = ["Admin", "Staff"];
        return await Context.UserRoles
            .Where(userRole => Context.Roles.Any(role => role.Id == userRole.RoleId && backOfficeRoles.Contains(role.Name!)))
            .Select(userRole => userRole.UserId)
            .Distinct()
            .Where(userId => Context.Users.Any(user => user.Id == userId && (user.LockoutEnd == null || user.LockoutEnd <= now)))
            .ToListAsync(cancellationToken);
    }

    public Task AddRangeAsync(IEnumerable<Notification> notifications, CancellationToken cancellationToken = default) =>
        DbSet.AddRangeAsync(notifications, cancellationToken);

    public Task<int> MarkAllReadAsync(string userId, DateTime readAt, CancellationToken cancellationToken = default) =>
        DbSet.Where(notification => notification.UserId == userId && !notification.IsRead)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(notification => notification.IsRead, true)
                .SetProperty(notification => notification.ReadAt, readAt), cancellationToken);
}
