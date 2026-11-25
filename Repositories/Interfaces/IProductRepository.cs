using PopPop.Data.Entities;
using PopPop.Models;

namespace PopPop.Repositories.Interfaces;

public interface IProductRepository : IRepository<Product, int>
{
    Task<IReadOnlyList<Product>> GetLatestAvailableAsync(int count, CancellationToken cancellationToken = default);
    /// <summary>One page of shop-visible products matching the filter (keyword, category, brand, specifications, price).</summary>
    Task<(IReadOnlyList<Product> Products, int TotalCount)> GetCatalogPageAsync(
        int pageNumber,
        int pageSize,
        CatalogFilter filter,
        string sortBy,
        CancellationToken cancellationToken = default);
    /// <summary>Active brands that have shop-visible products in the category (all active brands when null).</summary>
    Task<IReadOnlyList<Brand>> GetBrandsWithProductsAsync(int? categoryId, CancellationToken cancellationToken = default);
    Task<decimal> GetCatalogMaxPriceAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Category>> GetActiveCategoriesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Brand>> GetActiveBrandsAsync(CancellationToken cancellationToken = default);
    /// <summary>Active categories that currently have products for sale, with count and a sample image.</summary>
    Task<IReadOnlyList<CatalogGroupSummary>> GetCategorySummariesAsync(CancellationToken cancellationToken = default);
    /// <summary>Active brands that currently have products for sale, with count and a sample image.</summary>
    Task<IReadOnlyList<CatalogGroupSummary>> GetBrandSummariesAsync(CancellationToken cancellationToken = default);
    /// <summary>Available products ordered by quantity sold in non-cancelled orders.</summary>
    Task<IReadOnlyList<Product>> GetBestSellingAvailableAsync(int count, CancellationToken cancellationToken = default);
    Task<Product?> GetActiveDetailsAsync(int productId, CancellationToken cancellationToken = default);
    Task<ProductVariant?> GetAvailableVariantAsync(int variantId, CancellationToken cancellationToken = default);
}
