using PopPop.Models;

namespace PopPop.Services.Interfaces;

/// <summary>
/// Back-office product management. Business-rule violations throw <see cref="InvalidOperationException"/>
/// with a user-facing message; missing records throw <see cref="KeyNotFoundException"/>.
/// </summary>
public interface IProductManagementService
{
    Task<ProductManagementListViewModel> GetListAsync(
        string? search,
        int? categoryId,
        int? brandId,
        string? status,
        CancellationToken cancellationToken = default);

    Task PopulateFormOptionsAsync(ProductFormViewModel form, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(ProductFormViewModel form, IReadOnlyList<IFormFile> images, CancellationToken cancellationToken = default);
    Task<ProductEditViewModel?> GetEditAsync(int productId, ProductFormViewModel? postedForm = null, CancellationToken cancellationToken = default);
    Task UpdateAsync(ProductFormViewModel form, CancellationToken cancellationToken = default);
    Task DeleteAsync(int productId, CancellationToken cancellationToken = default);

    /// <summary>Replaces the product-level specifications (options must belong to the product's category).</summary>
    Task UpdateSpecsAsync(int productId, IReadOnlyCollection<int> optionIds, CancellationToken cancellationToken = default);

    /// <summary>Fills the variant-level specification selects of the form (used when redisplaying it).</summary>
    Task PopulateVariantSpecsAsync(ProductVariantFormViewModel form, CancellationToken cancellationToken = default);

    Task<ProductVariantFormViewModel?> GetVariantFormAsync(int productId, int? variantId, CancellationToken cancellationToken = default);
    Task CreateVariantAsync(ProductVariantFormViewModel form, CancellationToken cancellationToken = default);
    Task UpdateVariantAsync(ProductVariantFormViewModel form, CancellationToken cancellationToken = default);
    Task<int> DeleteVariantAsync(int variantId, CancellationToken cancellationToken = default);
    Task<int> AdjustStockAsync(StockAdjustmentViewModel adjustment, CancellationToken cancellationToken = default);

    Task UploadImagesAsync(int productId, IReadOnlyList<IFormFile> images, CancellationToken cancellationToken = default);
    Task<int> SetPrimaryImageAsync(int imageId, CancellationToken cancellationToken = default);
    Task<int> DeleteImageAsync(int imageId, CancellationToken cancellationToken = default);
}
