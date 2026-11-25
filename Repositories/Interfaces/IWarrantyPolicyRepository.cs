using PopPop.Data.Entities;

namespace PopPop.Repositories.Interfaces;

/// <summary>Admin configuration of warranty periods, receipt-defect windows and the defect catalogue.</summary>
public interface IWarrantyPolicyRepository
{
    Task<IReadOnlyList<Brand>> GetBrandsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Category>> GetCategoriesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WarrantyPolicy>> GetPoliciesAsync(CancellationToken cancellationToken = default);
    /// <summary>Number of products per (brand, category).</summary>
    Task<Dictionary<(int BrandId, int CategoryId), int>> GetProductCountsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DefectType>> GetDefectTypesAsync(CancellationToken cancellationToken = default);

    Task<Brand?> GetBrandAsync(int brandId, CancellationToken cancellationToken = default);
    Task<Category?> GetCategoryAsync(int categoryId, CancellationToken cancellationToken = default);
    /// <summary>Tracked policy row, or null when the pair has none yet.</summary>
    Task<WarrantyPolicy?> GetPolicyAsync(int brandId, int categoryId, CancellationToken cancellationToken = default);
    Task AddPolicyAsync(WarrantyPolicy policy, CancellationToken cancellationToken = default);
    Task<DefectType?> GetDefectTypeAsync(int defectTypeId, CancellationToken cancellationToken = default);
}
