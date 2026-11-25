using PopPop.Data.Entities;

namespace PopPop.Repositories.Interfaces;

public interface IProductManagementRepository : IRepository<Product, int>
{
    Task<IReadOnlyList<Product>> GetManagementListAsync(
        string? search,
        int? categoryId,
        int? brandId,
        bool? isActive,
        CancellationToken cancellationToken = default);
    Task<Product?> GetWithDetailsAsync(int productId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Category>> GetCategoriesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Brand>> GetBrandsAsync(CancellationToken cancellationToken = default);
    Task<bool> CategoryExistsAsync(int categoryId, CancellationToken cancellationToken = default);
    Task<bool> BrandExistsAsync(int brandId, CancellationToken cancellationToken = default);
    Task<bool> IsSkuTakenAsync(string sku, int? excludeVariantId, CancellationToken cancellationToken = default);
    Task<ProductVariant?> GetVariantAsync(int variantId, CancellationToken cancellationToken = default);
    Task<ProductImage?> GetImageAsync(int imageId, CancellationToken cancellationToken = default);
    Task<bool> HasOrderHistoryAsync(IReadOnlyCollection<int> variantIds, CancellationToken cancellationToken = default);
    Task RemoveCartItemsAsync(IReadOnlyCollection<int> variantIds, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InventoryTransaction>> GetInventoryHistoryAsync(
        int productId,
        int count,
        CancellationToken cancellationToken = default);
    void RemoveVariant(ProductVariant variant);
    void RemoveImage(ProductImage image);
}
