using Microsoft.EntityFrameworkCore;
using PopPop.Data;
using PopPop.Data.Entities.Enums;
using PopPop.Models;
using PopPop.Services.Interfaces;

namespace PopPop.Services.Implementations;

public sealed class HeaderBadgeService(IDbContextFactory<PopPopDbContext> dbFactory) : IHeaderBadgeService
{
    private const int RecentNotificationCount = 5;

    public async Task<HeaderBadges> GetCountsAsync(string userId, CancellationToken cancellationToken = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);

        var cartQuantity = await db.CartItems
            .Where(item => item.Cart!.UserId == userId)
            .Select(item => (int?)item.Quantity)
            .SumAsync(cancellationToken) ?? 0;

        var unreadNotificationCount = await db.Notifications
            .CountAsync(notification => notification.UserId == userId && !notification.IsRead, cancellationToken);

        var recent = await db.Notifications.AsNoTracking()
            .Where(notification => notification.UserId == userId)
            .OrderByDescending(notification => notification.CreatedAt)
            .ThenByDescending(notification => notification.Id)
            .Take(RecentNotificationCount)
            .ToListAsync(cancellationToken);

        return new HeaderBadges
        {
            CartQuantity = cartQuantity,
            UnreadNotificationCount = unreadNotificationCount,
            RecentNotifications = recent.Select(NotificationService.ToItem).ToList()
        };
    }

    public async Task<StaffBadges> GetStaffBadgesAsync(string userId, CancellationToken cancellationToken = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);

        var unread = await db.Notifications
            .CountAsync(notification => notification.UserId == userId && !notification.IsRead, cancellationToken);
        var recent = await db.Notifications.AsNoTracking()
            .Where(notification => notification.UserId == userId)
            .OrderByDescending(notification => notification.CreatedAt)
            .ThenByDescending(notification => notification.Id)
            .Take(RecentNotificationCount)
            .ToListAsync(cancellationToken);
        var pendingClaims = await db.WarrantyClaims
            .CountAsync(claim => claim.Status == ClaimStatus.Submitted, cancellationToken);
        var pendingOrders = await db.Orders
            .CountAsync(order => order.Status == OrderStatus.Pending, cancellationToken);

        return new StaffBadges
        {
            UnreadNotificationCount = unread,
            RecentNotifications = recent.Select(NotificationService.ToItem).ToList(),
            PendingClaimCount = pendingClaims,
            PendingOrderCount = pendingOrders
        };
    }
}
