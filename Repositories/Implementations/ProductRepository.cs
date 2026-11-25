using Microsoft.EntityFrameworkCore;
using PopPop.Data;
using PopPop.Data.Entities;
using PopPop.Data.Entities.Enums;
using PopPop.Models;
using PopPop.Repositories.Interfaces;

namespace PopPop.Repositories.Implementations;

public sealed class ProductRepository : Repository<Product, int>, IProductRepository
{
    public ProductRepository(PopPopDbContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<Product>> GetLatestAvailableAsync(
        int count,
        CancellationToken cancellationToken = default)
    {
        if (count <= 0)
        {
            return [];
        }

        return await DbSet
            .AsNoTracking()
            .AsSplitQuery()
            .Where(product => product.IsActive && product.Category!.IsActive && product.Brand!.IsActive &&
                product.ProductVariants.Any(variant => variant.IsActive && variant.StockQuantity > 0))
            .OrderByDescending(product => product.CreatedAt)
            .ThenByDescending(product => product.Id)
            .Take(count)
            .Include(product => product.ProductImages
                .OrderByDescending(image => image.IsPrimary)
                .ThenBy(image => image.SortOrder))
            .Include(product => product.ProductVariants
                .Where(variant => variant.IsActive && variant.StockQuantity > 0))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Brand>> GetBrandsWithProductsAsync(int? categoryId, CancellationToken cancellationToken = default)
    {
        var visible = CatalogFiltering.VisibleProducts(Context);
        if (categoryId.HasValue)
        {
            visible = visible.Where(product => product.CategoryId == categoryId.Value);
        }

        return await Context.Brands
            .AsNoTracking()
            .Where(brand => brand.IsActive && visible.Any(product => product.BrandId == brand.Id))
            .OrderBy(brand => brand.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<Product> Products, int TotalCount)> GetCatalogPageAsync(
        int pageNumber,
        int pageSize,
        CatalogFilter filter,
        string sortBy,
        CancellationToken cancellationToken = default)
    {
        var query = CatalogFiltering.VisibleProducts(Context).AsNoTracking().ApplyFilter(Context, filter);

        var totalCount = await query.CountAsync(cancellationToken);
        var orderedQuery = sortBy switch
        {
            "price-asc" => query
                .OrderBy(product => product.ProductVariants
                    .Where(variant => variant.IsActive && variant.StockQuantity > 0)
                    .Min(variant => (decimal?)variant.Price))
                .ThenBy(product => product.Id),
            "price-desc" => query
                .OrderByDescending(product => product.ProductVariants
                    .Where(variant => variant.IsActive && variant.StockQuantity > 0)
                    .Min(variant => (decimal?)variant.Price))
                .ThenByDescending(product => product.Id),
            "name" => query.OrderBy(product => product.Name).ThenBy(product => product.Id),
            _ => query.OrderByDescending(product => product.CreatedAt).ThenByDescending(product => product.Id)
        };

        var products = await orderedQuery
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsSplitQuery()
            .Include(product => product.ProductImages
                .OrderByDescending(image => image.IsPrimary)
                .ThenBy(image => image.SortOrder))
            .Include(product => product.ProductVariants
                .Where(variant => variant.IsActive && variant.StockQuantity > 0)
                .OrderBy(variant => variant.Price)
                .ThenBy(variant => variant.Id))
            .ToListAsync(cancellationToken);

        return (products, totalCount);
    }

    public async Task<IReadOnlyList<Category>> GetActiveCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await Context.Categories
            .AsNoTracking()
            .Where(category => category.IsActive)
            .OrderBy(category => category.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Brand>> GetActiveBrandsAsync(CancellationToken cancellationToken = default)
    {
        return await Context.Brands
            .AsNoTracking()
            .Where(brand => brand.IsActive)
            .OrderBy(brand => brand.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CatalogGroupSummary>> GetCategorySummariesAsync(CancellationToken cancellationToken = default)
    {
        var rows = await Context.Categories
            .AsNoTracking()
            .Where(category => category.IsActive)
            .Select(category => new
            {
                category.Id,
                category.Name,
                category.Description,
                ProductCount = category.Products.Count(product => product.IsActive && product.Category!.IsActive && product.Brand!.IsActive
                    && product.ProductVariants.Any(variant => variant.IsActive && variant.StockQuantity > 0)),
                ImageUrl = category.Products
                    .Where(product => product.IsActive && product.Category!.IsActive && product.Brand!.IsActive)
                    .OrderByDescending(product => product.CreatedAt)
                    .SelectMany(product => product.ProductImages.Where(image => image.IsPrimary))
                    .Select(image => image.ImageUrl)
                    .FirstOrDefault()
            })
            .Where(row => row.ProductCount > 0)
            .OrderByDescending(row => row.ProductCount)
            .ThenBy(row => row.Name)
            .ToListAsync(cancellationToken);

        return rows.Select(row => new CatalogGroupSummary(row.Id, row.Name, row.Description, row.ProductCount, row.ImageUrl)).ToList();
    }

    public async Task<IReadOnlyList<CatalogGroupSummary>> GetBrandSummariesAsync(CancellationToken cancellationToken = default)
    {
        var rows = await Context.Brands
            .AsNoTracking()
            .Where(brand => brand.IsActive)
            .Select(brand => new
            {
                brand.Id,
                brand.Name,
                brand.Description,
                ProductCount = brand.Products.Count(product => product.IsActive && product.Category!.IsActive && product.Brand!.IsActive
                    && product.ProductVariants.Any(variant => variant.IsActive && variant.StockQuantity > 0)),
                ImageUrl = brand.Products
                    .Where(product => product.IsActive && product.Category!.IsActive && product.Brand!.IsActive)
                    .OrderByDescending(product => product.CreatedAt)
                    .SelectMany(product => product.ProductImages.Where(image => image.IsPrimary))
                    .Select(image => image.ImageUrl)
                    .FirstOrDefault()
            })
            .Where(row => row.ProductCount > 0)
            .OrderByDescending(row => row.ProductCount)
            .ThenBy(row => row.Name)
            .ToListAsync(cancellationToken);

        return rows.Select(row => new CatalogGroupSummary(row.Id, row.Name, row.Description, row.ProductCount, row.ImageUrl)).ToList();
    }

    public async Task<IReadOnlyList<Product>> GetBestSellingAvailableAsync(
        int count,
        CancellationToken cancellationToken = default)
    {
        if (count <= 0)
        {
            return [];
        }

        var ranking = await Context.OrderItems
            .AsNoTracking()
            .Where(item => item.Order != null
                && item.Order.Status != OrderStatus.Cancelled
                && item.ProductVariant != null)
            .GroupBy(item => item.ProductVariant!.ProductId)
            .Select(group => new { ProductId = group.Key, Quantity = group.Sum(item => item.Quantity) })
            .OrderByDescending(row => row.Quantity)
            .Take(count * 2)
            .ToListAsync(cancellationToken);

        if (ranking.Count == 0)
        {
            return [];
        }

        var productIds = ranking.Select(row => row.ProductId).ToList();
        var products = await DbSet
            .AsNoTracking()
            .AsSplitQuery()
            .Where(product => productIds.Contains(product.Id)
                && product.IsActive && product.Category!.IsActive && product.Brand!.IsActive
                && product.ProductVariants.Any(variant => variant.IsActive && variant.StockQuantity > 0))
            .Include(product => product.ProductImages
                .OrderByDescending(image => image.IsPrimary)
                .ThenBy(image => image.SortOrder))
            .Include(product => product.ProductVariants
                .Where(variant => variant.IsActive && variant.StockQuantity > 0))
            .ToListAsync(cancellationToken);

        return products
            .OrderBy(product => productIds.IndexOf(product.Id))
            .Take(count)
            .ToList();
    }

    public async Task<decimal> GetCatalogMaxPriceAsync(CancellationToken cancellationToken = default)
    {
        return await Context.ProductVariants
            .AsNoTracking()
            .Where(variant => variant.IsActive
                && variant.StockQuantity > 0
                && variant.Product != null
                && variant.Product.IsActive && variant.Product.Category!.IsActive && variant.Product.Brand!.IsActive)
            .Select(variant => (decimal?)variant.Price)
            .MaxAsync(cancellationToken) ?? 0m;
    }

    public async Task<ProductVariant?> GetAvailableVariantAsync(
        int variantId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<ProductVariant>()
            .AsNoTracking()
            .FirstOrDefaultAsync(variant =>
                variant.Id == variantId &&
                variant.IsActive &&
                variant.StockQuantity > 0 &&
                variant.Product != null &&
                variant.Product.IsActive && variant.Product.Category!.IsActive && variant.Product.Brand!.IsActive,
                cancellationToken);
    }

    public async Task<Product?> GetActiveDetailsAsync(int productId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .AsSplitQuery()
            .Where(product => product.Id == productId && product.IsActive && product.Category!.IsActive && product.Brand!.IsActive)
            .Include(product => product.Category)
            .Include(product => product.Brand)
            .Include(product => product.ProductImages
                .OrderByDescending(image => image.IsPrimary)
                .ThenBy(image => image.SortOrder))
            .Include(product => product.ProductVariants
                .OrderBy(variant => variant.VariantLabel)
                .ThenBy(variant => variant.SKU))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
