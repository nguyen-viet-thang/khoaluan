using Microsoft.EntityFrameworkCore;
using PopPop.Data;
using PopPop.Data.Entities;
using PopPop.Data.Entities.Enums;
using PopPop.Models;
using PopPop.Repositories.Interfaces;

namespace PopPop.Repositories.Implementations;

public sealed class OrderRepository : Repository<Order, int>, IOrderRepository
{
    public OrderRepository(PopPopDbContext context)
        : base(context)
    {
    }

    public async Task<Order?> GetDetailsForUserAsync(
        int orderId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Include(order => order.OrderItems)
            .FirstOrDefaultAsync(order => order.Id == orderId && order.UserId == userId, cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetForUserAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking()
            .Where(order => order.UserId == userId)
            .OrderByDescending(order => order.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Order?> GetLatestForUserAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking()
            .Where(order => order.UserId == userId)
            .OrderByDescending(order => order.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<bool> OrderCodeExistsAsync(string orderCode, CancellationToken cancellationToken = default) =>
        DbSet.AnyAsync(order => order.OrderCode == orderCode, cancellationToken);

    public async Task<IReadOnlyList<Order>> GetForManagementAsync(
        OrderStatus? status,
        string? search,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking().Include(order => order.User).AsQueryable();
        if (status.HasValue)
        {
            query = query.Where(order => order.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(order => order.OrderCode.Contains(term)
                || (order.RecipientName != null && order.RecipientName.Contains(term))
                || (order.RecipientPhone != null && order.RecipientPhone.Contains(term))
                || (order.User != null && order.User.FullName != null && order.User.FullName.Contains(term))
                || (order.User != null && order.User.UserName != null && order.User.UserName.Contains(term)));
        }

        // Work queue order: the status that needs action first comes first
        // (Chờ xác nhận → Đã xác nhận → Đang xử lý → Đang giao → Đã giao → Đã hủy).
        // Open orders: oldest first, so the customer who has waited longest is served first.
        // Finished orders (completed / cancelled): newest first, they are only looked up.
        return await query
            .OrderBy(order => order.Status == OrderStatus.Pending ? 0
                : order.Status == OrderStatus.Confirmed ? 1
                : order.Status == OrderStatus.Processing ? 2
                : order.Status == OrderStatus.Shipping ? 3
                : order.Status == OrderStatus.Completed ? 4
                : 5)
            .ThenBy(order => order.Status == OrderStatus.Completed || order.Status == OrderStatus.Cancelled
                ? DateTime.MaxValue
                : order.CreatedAt)
            .ThenByDescending(order => order.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetRecentForManagementAsync(
        int count,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking()
            .Include(order => order.User)
            .OrderByDescending(order => order.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<Order?> GetDetailsForManagementAsync(
        int orderId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(order => order.User)
            .Include(order => order.OrderItems)
                .ThenInclude(item => item.ProductVariant)
            .FirstOrDefaultAsync(order => order.Id == orderId, cancellationToken);
    }

    public Task<int> CountByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default)
    {
        return DbSet.CountAsync(order => order.Status == status, cancellationToken);
    }

    public Task<int> CountAllAsync(CancellationToken cancellationToken = default)
    {
        return DbSet.CountAsync(cancellationToken);
    }
}
