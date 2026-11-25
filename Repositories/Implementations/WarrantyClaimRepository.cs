using Microsoft.EntityFrameworkCore;
using PopPop.Data;
using PopPop.Data.Entities;
using PopPop.Data.Entities.Enums;
using PopPop.Repositories.Interfaces;

namespace PopPop.Repositories.Implementations;

public sealed class WarrantyClaimRepository : Repository<WarrantyClaim, int>, IWarrantyClaimRepository
{
    private static readonly ClaimStatus[] ClosedStatuses = [ClaimStatus.Completed, ClaimStatus.Rejected, ClaimStatus.Cancelled];

    public WarrantyClaimRepository(PopPopDbContext context)
        : base(context)
    {
    }

    public async Task<Order?> GetOrderForClaimAsync(int orderId, string userId, CancellationToken cancellationToken = default)
    {
        return await OrdersWithClaimDetails()
            .FirstOrDefaultAsync(order => order.Id == orderId && order.UserId == userId, cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetDeliveredOrdersForClaimAsync(string userId, string? search, CancellationToken cancellationToken = default)
    {
        var query = OrdersWithClaimDetails()
            .Where(order => order.UserId == userId && order.Status == OrderStatus.Completed);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(order => order.OrderCode.Contains(term)
                || order.OrderItems.Any(item => item.ProductName.Contains(term) || item.SKU.Contains(term)));
        }

        // Delivery time = PaidAt for COD orders (fallback UpdatedAt), same as the claim deadlines.
        return await query
            .OrderByDescending(order => order.PaidAt ?? order.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountUndeliveredOrdersAsync(string userId, CancellationToken cancellationToken = default) =>
        Context.Orders.CountAsync(order => order.UserId == userId
            && order.Status != OrderStatus.Completed && order.Status != OrderStatus.Cancelled, cancellationToken);

    private IQueryable<Order> OrdersWithClaimDetails()
    {
        return Context.Orders
            .AsNoTracking()
            .AsSplitQuery()
            .Include(order => order.OrderItems)
                .ThenInclude(item => item.ProductVariant)
                    .ThenInclude(variant => variant!.Product)
                        .ThenInclude(product => product!.Category)
            .Include(order => order.OrderItems)
                .ThenInclude(item => item.ProductVariant)
                    .ThenInclude(variant => variant!.Product)
                        .ThenInclude(product => product!.Brand)
            .Include(order => order.OrderItems)
                .ThenInclude(item => item.ProductVariant)
                    .ThenInclude(variant => variant!.Product)
                        .ThenInclude(product => product!.ProductImages.Where(image => image.IsPrimary));
    }

    public async Task<WarrantyPolicy?> GetPolicyAsync(int brandId, int categoryId, CancellationToken cancellationToken = default)
    {
        return await Context.WarrantyPolicies.AsNoTracking()
            .FirstOrDefaultAsync(policy => policy.BrandId == brandId && policy.CategoryId == categoryId, cancellationToken);
    }

    public async Task<IReadOnlyList<WarrantyPolicy>> GetPoliciesAsync(IReadOnlyCollection<int> brandIds, CancellationToken cancellationToken = default)
    {
        return await Context.WarrantyPolicies.AsNoTracking()
            .Where(policy => brandIds.Contains(policy.BrandId))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DefectType>> GetActiveDefectTypesAsync(
        IReadOnlyCollection<int> categoryIds,
        CancellationToken cancellationToken = default)
    {
        return await Context.DefectTypes.AsNoTracking()
            .Where(defect => defect.IsActive && categoryIds.Contains(defect.CategoryId))
            .OrderBy(defect => defect.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<DefectType?> GetDefectTypeAsync(int defectTypeId, CancellationToken cancellationToken = default)
    {
        return await Context.DefectTypes.AsNoTracking().FirstOrDefaultAsync(defect => defect.Id == defectTypeId, cancellationToken);
    }

    public async Task<Dictionary<int, string>> GetOpenClaimCodesAsync(
        IReadOnlyCollection<int> orderItemIds,
        CancellationToken cancellationToken = default)
    {
        var rows = await DbSet.AsNoTracking()
            .Where(claim => orderItemIds.Contains(claim.OrderItemId) && !ClosedStatuses.Contains(claim.Status))
            .Select(claim => new { claim.OrderItemId, claim.Code })
            .ToListAsync(cancellationToken);
        return rows.GroupBy(row => row.OrderItemId).ToDictionary(group => group.Key, group => group.First().Code);
    }

    public async Task<IReadOnlyList<WarrantyClaim>> GetForUserAsync(string userId, int? orderId, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking()
            .Include(claim => claim.OrderItem)
            .Include(claim => claim.DefectType)
            .Include(claim => claim.Order)
            .Where(claim => claim.UserId == userId && (!orderId.HasValue || claim.OrderId == orderId.Value))
            .OrderByDescending(claim => claim.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<WarrantyClaim>> GetForStaffAsync(
        ClaimStatus? status,
        ClaimType? type,
        string? search,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking()
            .Include(claim => claim.OrderItem)
            .Include(claim => claim.DefectType)
            .Include(claim => claim.Order)
            .Include(claim => claim.User)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(claim => claim.Status == status.Value);
        }

        if (type.HasValue)
        {
            query = query.Where(claim => claim.Type == type.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(claim => claim.Code.Contains(term)
                || claim.Order!.OrderCode.Contains(term)
                || claim.OrderItem!.ProductName.Contains(term)
                || (claim.User!.FullName != null && claim.User.FullName.Contains(term))
                || (claim.User.UserName != null && claim.User.UserName.Contains(term))
                || (claim.Order.RecipientPhone != null && claim.Order.RecipientPhone.Contains(term)));
        }

        // Open claims first (oldest waiting first), then closed ones (newest first).
        return await query
            .OrderBy(claim => ClosedStatuses.Contains(claim.Status))
            .ThenBy(claim => ClosedStatuses.Contains(claim.Status) ? DateTime.MaxValue : claim.UpdatedAt)
            .ThenByDescending(claim => claim.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountOpenAsync(CancellationToken cancellationToken = default) =>
        DbSet.CountAsync(claim => !ClosedStatuses.Contains(claim.Status), cancellationToken);

    public async Task<WarrantyClaim?> GetDetailsAsync(int claimId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsSplitQuery()
            .Include(claim => claim.Order)
            .Include(claim => claim.OrderItem)
                .ThenInclude(item => item!.ProductVariant)
                    .ThenInclude(variant => variant!.Product)
                        .ThenInclude(product => product!.Brand)
            .Include(claim => claim.DefectType)
            .Include(claim => claim.User)
            .Include(claim => claim.Media.OrderBy(media => media.UploadedAt))
            .Include(claim => claim.History.OrderBy(history => history.CreatedAt))
            .FirstOrDefaultAsync(claim => claim.Id == claimId, cancellationToken);
    }

    public async Task<WarrantyClaimMedia?> GetMediaAsync(int mediaId, CancellationToken cancellationToken = default)
    {
        return await Context.WarrantyClaimMedia.AsNoTracking()
            .Include(media => media.Claim)
            .FirstOrDefaultAsync(media => media.Id == mediaId, cancellationToken);
    }

    public Task<bool> ClaimCodeExistsAsync(string code, CancellationToken cancellationToken = default) =>
        DbSet.AnyAsync(claim => claim.Code == code, cancellationToken);

    public async Task AddInventoryTransactionAsync(InventoryTransaction transaction, CancellationToken cancellationToken = default)
    {
        await Context.InventoryTransactions.AddAsync(transaction, cancellationToken);
    }
}
