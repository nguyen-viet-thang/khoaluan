using PopPop.Data.Entities;
using PopPop.Data.Entities.Enums;
using PopPop.Models;

namespace PopPop.Repositories.Interfaces;

public interface IOrderRepository : IRepository<Order, int>
{
    Task<Order?> GetDetailsForUserAsync(int orderId, string userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetForUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<Order?> GetLatestForUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> OrderCodeExistsAsync(string orderCode, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetForManagementAsync(OrderStatus? status, string? search, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetRecentForManagementAsync(int count, CancellationToken cancellationToken = default);
    Task<Order?> GetDetailsForManagementAsync(int orderId, CancellationToken cancellationToken = default);
    Task<int> CountAllAsync(CancellationToken cancellationToken = default);
    Task<int> CountByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default);
    /// <summary>Number of orders per status among those matching the search (all orders when it is empty).</summary>
    Task<IReadOnlyDictionary<OrderStatus, int>> CountByStatusAsync(string? search, CancellationToken cancellationToken = default);
    /// <summary>Tracked VNPAY orders still waiting for payment that were placed before the given instant (with items + variants).</summary>
    Task<IReadOnlyList<Order>> GetUnpaidOnlineOrdersCreatedBeforeAsync(DateTime createdBeforeUtc, CancellationToken cancellationToken = default);
}
