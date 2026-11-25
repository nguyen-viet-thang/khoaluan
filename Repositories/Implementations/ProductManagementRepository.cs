using Microsoft.EntityFrameworkCore;
using PopPop.Data;
using PopPop.Data.Entities;
using PopPop.Repositories.Interfaces;

namespace PopPop.Repositories.Implementations;

public sealed class ProductManagementRepository : Repository<Product, int>, IProductManagementRepository
{
    public ProductManagementRepository(PopPopDbContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<Product>> GetManagementListAsync(
        string? search,
        int? categoryId,
        int? brandId,
        bool? isActive,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim();
            query = query.Where(product => product.Name.Contains(keyword)
                || product.ProductVariants.Any(variant => variant.SKU.Contains(keyword)));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(product => product.CategoryId == categoryId.Value);
        }

        if (brandId.HasValue)
        {
            query = query.Where(product => product.BrandId == brandId.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(product => product.IsActive == isActive.Value);
        }

        return await query
            .OrderByDescending(product => product.UpdatedAt)
            .ThenByDescending(product => product.Id)
            .AsSplitQuery()
            .Include(product => product.Category)
            .Include(product => product.Brand)
            .Include(product => product.ProductVariants)
            .Include(product => product.ProductImages
                .OrderByDescending(image => image.IsPrimary)
                .ThenBy(image => image.SortOrder))
            .ToListAsync(cancellationToken);
    }

    public async Task<Product?> GetWithDetailsAsync(int productId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsSplitQuery()
            .Include(product => product.Category)
            .Include(product => product.Brand)
            .Include(product => product.ProductVariants)
            .Include(product => product.ProductImages)
            .FirstOrDefaultAsync(product => product.Id == productId, cancellationToken);
    }

    public async Task<IReadOnlyList<Category>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await Context.Categories
            .AsNoTracking()
            .OrderBy(category => category.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Brand>> GetBrandsAsync(CancellationToken cancellationToken = default)
    {
        return await Context.Brands
            .AsNoTracking()
            .OrderBy(brand => brand.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> CategoryExistsAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        return await Context.Categories.AnyAsync(category => category.Id == categoryId, cancellationToken);
    }

    public async Task<bool> BrandExistsAsync(int brandId, CancellationToken cancellationToken = default)
    {
        return await Context.Brands.AnyAsync(brand => brand.Id == brandId, cancellationToken);
    }

    public async Task<bool> IsSkuTakenAsync(
        string sku,
        int? excludeVariantId,
        CancellationToken cancellationToken = default)
    {
        return await Context.ProductVariants.AnyAsync(
            variant => variant.SKU == sku && (!excludeVariantId.HasValue || variant.Id != excludeVariantId.Value),
            cancellationToken);
    }

    public async Task<ProductVariant?> GetVariantAsync(int variantId, CancellationToken cancellationToken = default)
    {
        return await Context.ProductVariants
            .Include(variant => variant.Product)
            .FirstOrDefaultAsync(variant => variant.Id == variantId, cancellationToken);
    }

    public async Task<ProductImage?> GetImageAsync(int imageId, CancellationToken cancellationToken = default)
    {
        return await Context.ProductImages.FirstOrDefaultAsync(image => image.Id == imageId, cancellationToken);
    }

    public async Task<bool> HasOrderHistoryAsync(
        IReadOnlyCollection<int> variantIds,
        CancellationToken cancellationToken = default)
    {
        if (variantIds.Count == 0)
        {
            return false;
        }

        return await Context.OrderItems.AnyAsync(
            item => variantIds.Contains(item.ProductVariantId),
            cancellationToken);
    }

    public async Task RemoveCartItemsAsync(
        IReadOnlyCollection<int> variantIds,
        CancellationToken cancellationToken = default)
    {
        if (variantIds.Count == 0)
        {
            return;
        }

        await Context.CartItems
            .Where(item => variantIds.Contains(item.ProductVariantId))
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<InventoryTransaction>> GetInventoryHistoryAsync(
        int productId,
        int count,
        CancellationToken cancellationToken = default)
    {
        return await Context.InventoryTransactions
            .AsNoTracking()
            .Include(transaction => transaction.ProductVariant)
            .Where(transaction => transaction.ProductVariant != null
                && transaction.ProductVariant.ProductId == productId)
            .OrderByDescending(transaction => transaction.CreatedAt)
            .ThenByDescending(transaction => transaction.Id)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public void RemoveVariant(ProductVariant variant)
    {
        Context.ProductVariants.Remove(variant);
    }

    public void RemoveImage(ProductImage image)
    {
        Context.ProductImages.Remove(image);
    }
}
