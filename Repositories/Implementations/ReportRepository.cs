using Microsoft.EntityFrameworkCore;
using PopPop.Data;
using PopPop.Data.Entities.Enums;
using PopPop.Models;
using PopPop.Repositories.Interfaces;

namespace PopPop.Repositories.Implementations;

public sealed class ReportRepository : IReportRepository
{
    private static readonly string[] BackOfficeRoles = ["Admin", "Staff"];
    private readonly PopPopDbContext _context;

    public ReportRepository(PopPopDbContext context)
    {
        _context = context;
    }

    // A COD order is paid when it is delivered, so PaidAt is the completion time.
    // UpdatedAt is the fallback for orders completed before PaidAt was recorded.

    public async Task<IReadOnlyList<ReportOrderRow>> GetOrdersAsync(
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .AsNoTracking()
            .Where(order => (order.CreatedAt >= fromUtc && order.CreatedAt < toUtc)
                || (order.Status == OrderStatus.Completed
                    && (order.PaidAt ?? order.UpdatedAt) >= fromUtc
                    && (order.PaidAt ?? order.UpdatedAt) < toUtc))
            .OrderBy(order => order.CreatedAt)
            .Select(order => new ReportOrderRow(
                order.Id,
                order.OrderCode,
                order.CreatedAt,
                order.Status == OrderStatus.Completed ? order.PaidAt ?? order.UpdatedAt : null,
                order.Status,
                order.TotalAmount,
                order.ShippingFee,
                order.User != null ? order.User.FullName ?? order.RecipientName : order.RecipientName,
                order.RecipientPhone,
                order.OrderItems.Sum(item => item.Quantity)))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ReportSoldItemRow>> GetSoldItemsAsync(
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default)
    {
        return await _context.OrderItems
            .AsNoTracking()
            .Where(item => item.Order != null
                && item.Order.Status == OrderStatus.Completed
                && (item.Order.PaidAt ?? item.Order.UpdatedAt) >= fromUtc
                && (item.Order.PaidAt ?? item.Order.UpdatedAt) < toUtc)
            .Select(item => new ReportSoldItemRow(
                item.ProductVariant!.ProductId,
                item.ProductVariant.Product!.Name,
                item.ProductVariant.Product.Category!.Name,
                item.Quantity,
                item.TotalPrice))
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetCompletedRevenueAsync(
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Where(order => order.Status == OrderStatus.Completed
                && (order.PaidAt ?? order.UpdatedAt) >= fromUtc
                && (order.PaidAt ?? order.UpdatedAt) < toUtc)
            .SumAsync(order => (decimal?)order.TotalAmount, cancellationToken) ?? 0m;
    }

    public async Task<int> CountNewCustomersAsync(
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default)
    {
        var backOfficeUserIds = _context.UserRoles
            .Where(userRole => _context.Roles.Any(role => role.Id == userRole.RoleId && BackOfficeRoles.Contains(role.Name!)))
            .Select(userRole => userRole.UserId);

        return await _context.Users.CountAsync(user => user.CreatedAt >= fromUtc
            && user.CreatedAt < toUtc
            && !backOfficeUserIds.Contains(user.Id), cancellationToken);
    }

    public async Task<IReadOnlyList<LowStockRow>> GetLowStockAsync(int threshold, CancellationToken cancellationToken = default)
    {
        return await _context.ProductVariants
            .AsNoTracking()
            .Where(variant => variant.IsActive
                && variant.StockQuantity <= threshold
                && variant.Product != null
                && variant.Product.IsActive)
            .OrderBy(variant => variant.StockQuantity)
            .ThenBy(variant => variant.Product!.Name)
            .Select(variant => new LowStockRow(
                variant.ProductId,
                variant.Product!.Name,
                variant.SKU,
                variant.VariantLabel,
                variant.StockQuantity,
                variant.Product.IsActive))
            .ToListAsync(cancellationToken);
    }
}
