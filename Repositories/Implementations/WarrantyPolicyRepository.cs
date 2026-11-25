using Microsoft.EntityFrameworkCore;
using PopPop.Data;
using PopPop.Data.Entities;
using PopPop.Repositories.Interfaces;

namespace PopPop.Repositories.Implementations;

public sealed class WarrantyPolicyRepository : IWarrantyPolicyRepository
{
    private readonly PopPopDbContext _context;

    public WarrantyPolicyRepository(PopPopDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Brand>> GetBrandsAsync(CancellationToken cancellationToken = default) =>
        await _context.Brands.AsNoTracking().OrderBy(brand => brand.Name).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Category>> GetCategoriesAsync(CancellationToken cancellationToken = default) =>
        await _context.Categories.AsNoTracking().OrderBy(category => category.Id).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<WarrantyPolicy>> GetPoliciesAsync(CancellationToken cancellationToken = default) =>
        await _context.WarrantyPolicies.AsNoTracking().ToListAsync(cancellationToken);

    public async Task<Dictionary<(int BrandId, int CategoryId), int>> GetProductCountsAsync(CancellationToken cancellationToken = default)
    {
        var rows = await _context.Products.AsNoTracking()
            .GroupBy(product => new { product.BrandId, product.CategoryId })
            .Select(group => new { group.Key.BrandId, group.Key.CategoryId, Count = group.Count() })
            .ToListAsync(cancellationToken);
        return rows.ToDictionary(row => (row.BrandId, row.CategoryId), row => row.Count);
    }

    public async Task<IReadOnlyList<DefectType>> GetDefectTypesAsync(CancellationToken cancellationToken = default) =>
        await _context.DefectTypes.AsNoTracking()
            .Include(defect => defect.Category)
            .OrderBy(defect => defect.CategoryId)
            .ThenBy(defect => defect.SortOrder)
            .ToListAsync(cancellationToken);

    public Task<Brand?> GetBrandAsync(int brandId, CancellationToken cancellationToken = default) =>
        _context.Brands.FirstOrDefaultAsync(brand => brand.Id == brandId, cancellationToken);

    public Task<Category?> GetCategoryAsync(int categoryId, CancellationToken cancellationToken = default) =>
        _context.Categories.FirstOrDefaultAsync(category => category.Id == categoryId, cancellationToken);

    public Task<WarrantyPolicy?> GetPolicyAsync(int brandId, int categoryId, CancellationToken cancellationToken = default) =>
        _context.WarrantyPolicies.FirstOrDefaultAsync(policy => policy.BrandId == brandId && policy.CategoryId == categoryId, cancellationToken);

    public async Task AddPolicyAsync(WarrantyPolicy policy, CancellationToken cancellationToken = default) =>
        await _context.WarrantyPolicies.AddAsync(policy, cancellationToken);

    public Task<DefectType?> GetDefectTypeAsync(int defectTypeId, CancellationToken cancellationToken = default) =>
        _context.DefectTypes.Include(defect => defect.Category).FirstOrDefaultAsync(defect => defect.Id == defectTypeId, cancellationToken);
}
