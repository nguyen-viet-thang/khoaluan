using PopPop.Data.Entities;
using PopPop.Data.Entities.Enums;

namespace PopPop.Repositories.Interfaces;

public interface IWarrantyClaimRepository : IRepository<WarrantyClaim, int>
{
    /// <summary>Customer's order with lines, products, categories, brands and images (read-only).</summary>
    Task<Order?> GetOrderForClaimAsync(int orderId, string userId, CancellationToken cancellationToken = default);
    /// <summary>
    /// The user's delivered (completed) orders, newest delivery first, loaded like <see cref="GetOrderForClaimAsync"/>.
    /// <paramref name="search"/> matches the order code or a product name / SKU.
    /// </summary>
    Task<IReadOnlyList<Order>> GetDeliveredOrdersForClaimAsync(string userId, string? search, CancellationToken cancellationToken = default);
    /// <summary>The user's orders that are placed but not delivered yet (not completed, not cancelled).</summary>
    Task<int> CountUndeliveredOrdersAsync(string userId, CancellationToken cancellationToken = default);

    Task<WarrantyPolicy?> GetPolicyAsync(int brandId, int categoryId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WarrantyPolicy>> GetPoliciesAsync(IReadOnlyCollection<int> brandIds, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DefectType>> GetActiveDefectTypesAsync(IReadOnlyCollection<int> categoryIds, CancellationToken cancellationToken = default);
    Task<DefectType?> GetDefectTypeAsync(int defectTypeId, CancellationToken cancellationToken = default);

    /// <summary>Open (not closed) claim code per order line.</summary>
    Task<Dictionary<int, string>> GetOpenClaimCodesAsync(IReadOnlyCollection<int> orderItemIds, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WarrantyClaim>> GetForUserAsync(string userId, int? orderId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WarrantyClaim>> GetForStaffAsync(ClaimStatus? status, ClaimType? type, string? search, CancellationToken cancellationToken = default);
    Task<int> CountOpenAsync(CancellationToken cancellationToken = default);

    /// <summary>Tracked claim with order, line, variant, product, brand, defect, customer, media and history.</summary>
    Task<WarrantyClaim?> GetDetailsAsync(int claimId, CancellationToken cancellationToken = default);
    Task<WarrantyClaimMedia?> GetMediaAsync(int mediaId, CancellationToken cancellationToken = default);
    Task<bool> ClaimCodeExistsAsync(string code, CancellationToken cancellationToken = default);
    Task AddInventoryTransactionAsync(InventoryTransaction transaction, CancellationToken cancellationToken = default);
}
