using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;
using PopPop.Data;
using PopPop.Data.Entities.Enums;
using PopPop.Models;
using PopPop.Services.Interfaces;

namespace PopPop.Services.Implementations;

public sealed class HeaderBadgeService(IDbContextFactory<PopPopDbContext> dbFactory, IMemoryCache cache) : IHeaderBadgeService
{
    private const int RecentNotificationCount = 5;
    private const string MenuCacheKey = "header-menu";
    private static readonly TimeSpan MenuCacheDuration = TimeSpan.FromMinutes(5);

    public async Task<HeaderMenu> GetMenuAsync(CancellationToken cancellationToken = default)
    {
        if (cache.TryGetValue(MenuCacheKey, out HeaderMenu? cached) && cached is not null)
        {
            return cached;
        }

        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        var onSale = db.Products.AsNoTracking()
            .Where(product => product.IsActive && product.Category!.IsActive && product.Brand!.IsActive);

        var categories = await onSale
            .GroupBy(product => new { product.CategoryId, product.Category!.Name })
            .Select(group => new HeaderMenuLink(group.Key.CategoryId, group.Key.Name, group.Count()))
            .ToListAsync(cancellationToken);
        var brands = await onSale
            .GroupBy(product => new { product.BrandId, product.Brand!.Name })
            .Select(group => new HeaderMenuLink(group.Key.BrandId, group.Key.Name, group.Count()))
            .ToListAsync(cancellationToken);

        var menu = new HeaderMenu
        {
            Categories = categories.OrderBy(link => link.Name).ToList(),
            Brands = brands.OrderByDescending(link => link.ProductCount).ThenBy(link => link.Name).ToList()
        };
        cache.Set(MenuCacheKey, menu, MenuCacheDuration);
        return menu;
    }

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
        // Unpaid VNPAY orders are not actionable yet (the customer still has to pay), so they are not counted.
        var pendingOrders = await db.Orders
            .CountAsync(order => order.Status == OrderStatus.Pending
                && !(order.PaymentMethod == PaymentMethod.VnPay && order.PaymentStatus != PaymentStatus.Paid), cancellationToken);

        return new StaffBadges
        {
            UnreadNotificationCount = unread,
            RecentNotifications = recent.Select(NotificationService.ToItem).ToList(),
            PendingClaimCount = pendingClaims,
            PendingOrderCount = pendingOrders
        };
    }
}
