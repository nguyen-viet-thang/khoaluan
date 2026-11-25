using PopPop.Data.Entities;
using PopPop.Data.Entities.Enums;
using PopPop.Models;
using PopPop.Repositories.Interfaces;
using PopPop.Services.Interfaces;

namespace PopPop.Services.Implementations;

public sealed class ProductManagementService : IProductManagementService
{
    public const int MaxImagesPerUpload = 10;
    private const int InventoryHistorySize = 30;

    private readonly IProductManagementRepository _repository;
    private readonly IProductImageStorage _imageStorage;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISpecRepository _specRepository;

    public ProductManagementService(
        IProductManagementRepository repository,
        IProductImageStorage imageStorage,
        IUnitOfWork unitOfWork,
        ISpecRepository specRepository)
    {
        _repository = repository;
        _imageStorage = imageStorage;
        _unitOfWork = unitOfWork;
        _specRepository = specRepository;
    }

    #region Product

    public async Task<ProductManagementListViewModel> GetListAsync(
        string? search,
        int? categoryId,
        int? brandId,
        string? status,
        CancellationToken cancellationToken = default)
    {
        var normalizedStatus = status is "active" or "inactive" ? status : null;
        bool? isActive = normalizedStatus switch
        {
            "active" => true,
            "inactive" => false,
            _ => null
        };
        var normalizedSearch = string.IsNullOrWhiteSpace(search) ? null : search.Trim();

        var products = await _repository.GetManagementListAsync(normalizedSearch, categoryId, brandId, isActive, cancellationToken);
        return new ProductManagementListViewModel
        {
            Products = products.Select(product => new ProductManagementItemViewModel
            {
                Id = product.Id,
                Name = product.Name,
                ImageUrl = product.ProductImages.FirstOrDefault()?.ImageUrl,
                CategoryName = product.Category?.Name ?? string.Empty,
                BrandName = product.Brand?.Name ?? string.Empty,
                VariantCount = product.ProductVariants.Count,
                TotalStock = product.ProductVariants.Sum(variant => variant.StockQuantity),
                MinPrice = product.ProductVariants.Count == 0 ? null : product.ProductVariants.Min(variant => variant.Price),
                MaxPrice = product.ProductVariants.Count == 0 ? null : product.ProductVariants.Max(variant => variant.Price),
                IsActive = product.IsActive,
                HiddenReason = GetHiddenReason(product.Category, product.Brand),
                UpdatedAt = product.UpdatedAt
            }).ToList(),
            Categories = await GetCategoryOptionsAsync(cancellationToken),
            Brands = await GetBrandOptionsAsync(cancellationToken),
            Search = normalizedSearch,
            CategoryId = categoryId,
            BrandId = brandId,
            Status = normalizedStatus
        };
    }

    public async Task PopulateFormOptionsAsync(ProductFormViewModel form, CancellationToken cancellationToken = default)
    {
        form.Categories = await GetCategoryOptionsAsync(cancellationToken);
        form.Brands = await GetBrandOptionsAsync(cancellationToken);
    }

    public async Task<int> CreateAsync(
        ProductFormViewModel form,
        IReadOnlyList<IFormFile> images,
        CancellationToken cancellationToken = default)
    {
        await EnsureCategoryAndBrandAsync(form, cancellationToken);
        await ValidateImagesAsync(images, cancellationToken);

        var savedUrls = await SaveImagesAsync(images, cancellationToken);
        try
        {
            var now = DateTime.UtcNow;
            var product = new Product
            {
                Name = form.Name.Trim(),
                Description = NormalizeOptional(form.Description),
                CategoryId = form.CategoryId!.Value,
                BrandId = form.BrandId!.Value,
                IsActive = form.IsActive,
                CreatedAt = now,
                UpdatedAt = now
            };

            for (var index = 0; index < savedUrls.Count; index++)
            {
                product.ProductImages.Add(new ProductImage
                {
                    ImageUrl = savedUrls[index],
                    IsPrimary = index == 0,
                    SortOrder = index + 1
                });
            }

            await _repository.AddAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return product.Id;
        }
        catch
        {
            DeleteFiles(savedUrls);
            throw;
        }
    }

    public async Task<ProductEditViewModel?> GetEditAsync(
        int productId,
        ProductFormViewModel? postedForm = null,
        CancellationToken cancellationToken = default)
    {
        var product = await _repository.GetWithDetailsAsync(productId, cancellationToken);
        if (product is null)
        {
            return null;
        }

        var form = postedForm ?? new ProductFormViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            CategoryId = product.CategoryId,
            BrandId = product.BrandId,
            IsActive = product.IsActive
        };
        form.Id = product.Id;
        await PopulateFormOptionsAsync(form, cancellationToken);

        var history = await _repository.GetInventoryHistoryAsync(productId, InventoryHistorySize, cancellationToken);
        var attributes = await _specRepository.GetAttributesAsync(product.CategoryId, cancellationToken);
        var (productValues, variantValues) = await _specRepository.GetValuesForProductAsync(productId, cancellationToken);
        var selectedProductOptions = productValues.Select(value => value.SpecOptionId).ToHashSet();
        var variantAttributes = attributes.Where(attribute => attribute.Scope == SpecScope.Variant).ToList();
        var variantOptions = variantValues
            .GroupBy(value => value.ProductVariantId)
            .ToDictionary(group => group.Key, group => group.Select(value => value.Option!).ToList());

        return new ProductEditViewModel
        {
            Form = form,
            HiddenReason = product.IsActive ? GetHiddenReason(product.Category, product.Brand) : null,
            ProductSpecs = attributes
                .Where(attribute => attribute.Scope == SpecScope.Product)
                .Select(attribute => ToEditorGroup(attribute, selectedProductOptions))
                .ToList(),
            VariantSpecNames = variantAttributes.Select(attribute => attribute.Name).ToList(),
            Variants = product.ProductVariants
                .OrderBy(variant => variant.VariantLabel)
                .ThenBy(variant => variant.SKU)
                .Select(variant =>
                {
                    var options = variantOptions.GetValueOrDefault(variant.Id) ?? [];
                    var setAttributes = options.Select(option => option.SpecAttributeId).ToHashSet();
                    return new ProductVariantItemViewModel
                    {
                        Id = variant.Id,
                        SKU = variant.SKU,
                        VariantLabel = variant.VariantLabel,
                        Price = variant.Price,
                        StockQuantity = variant.StockQuantity,
                        IsActive = variant.IsActive,
                        SpecSummary = options.Count == 0
                            ? null
                            : string.Join(" · ", options.OrderBy(option => option.Attribute!.SortOrder).Select(option => option.Value)),
                        IsMissingSpecs = variantAttributes.Any(attribute => !setAttributes.Contains(attribute.Id))
                    };
                }).ToList(),
            Images = product.ProductImages
                .OrderByDescending(image => image.IsPrimary)
                .ThenBy(image => image.SortOrder)
                .Select(image => new ProductImageItemViewModel
                {
                    Id = image.Id,
                    ImageUrl = image.ImageUrl,
                    IsPrimary = image.IsPrimary
                }).ToList(),
            InventoryHistory = history.Select(transaction => new InventoryHistoryItemViewModel
            {
                CreatedAt = transaction.CreatedAt,
                SKU = transaction.ProductVariant?.SKU ?? string.Empty,
                Type = transaction.Type,
                Quantity = transaction.Quantity,
                Note = transaction.Note
            }).ToList()
        };
    }

    public async Task UpdateAsync(ProductFormViewModel form, CancellationToken cancellationToken = default)
    {
        var product = await _repository.GetByIdAsync(form.Id ?? 0, cancellationToken)
            ?? throw new KeyNotFoundException();
        await EnsureCategoryAndBrandAsync(form, cancellationToken);

        if (product.CategoryId != form.CategoryId!.Value)
        {
            // Specifications are defined per category: the old category's values no longer apply.
            await _specRepository.RemoveValuesOutsideCategoryAsync(product.Id, form.CategoryId.Value, cancellationToken);
        }

        product.Name = form.Name.Trim();
        product.Description = NormalizeOptional(form.Description);
        product.CategoryId = form.CategoryId!.Value;
        product.BrandId = form.BrandId!.Value;
        product.IsActive = form.IsActive;
        product.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int productId, CancellationToken cancellationToken = default)
    {
        var imageUrls = await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var product = await _repository.GetWithDetailsAsync(productId, token)
                ?? throw new KeyNotFoundException();
            var variantIds = product.ProductVariants.Select(variant => variant.Id).ToList();
            if (await _repository.HasOrderHistoryAsync(variantIds, token))
            {
                throw new InvalidOperationException(
                    "Sản phẩm đã phát sinh đơn hàng nên không thể xóa. Hãy chuyển sang trạng thái \"Ngừng bán\" để ẩn khỏi cửa hàng.");
            }

            var urls = product.ProductImages.Select(image => image.ImageUrl).ToList();
            await _repository.RemoveCartItemsAsync(variantIds, token);
            _repository.Delete(product);
            return urls;
        }, cancellationToken);

        DeleteFiles(imageUrls);
    }

    public async Task UpdateSpecsAsync(int productId, IReadOnlyCollection<int> optionIds, CancellationToken cancellationToken = default)
    {
        var product = await _repository.GetByIdAsync(productId, cancellationToken)
            ?? throw new KeyNotFoundException();
        var options = await ValidateSpecSelectionAsync(product.CategoryId, SpecScope.Product, optionIds, cancellationToken);
        await _specRepository.ReplaceProductValuesAsync(product.Id, options.Select(option => option.Id).ToList(), cancellationToken);
        product.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    #endregion

    #region Variant & inventory

    public async Task<ProductVariantFormViewModel?> GetVariantFormAsync(
        int productId,
        int? variantId,
        CancellationToken cancellationToken = default)
    {
        var product = await _repository.GetByIdAsync(productId, cancellationToken);
        if (product is null)
        {
            return null;
        }

        if (!variantId.HasValue)
        {
            var newForm = new ProductVariantFormViewModel { ProductId = product.Id, ProductName = product.Name };
            await PopulateVariantSpecsAsync(newForm, cancellationToken);
            return newForm;
        }

        var variant = await _repository.GetVariantAsync(variantId.Value, cancellationToken);
        if (variant is null || variant.ProductId != productId)
        {
            return null;
        }

        var (_, variantValues) = await _specRepository.GetValuesForProductAsync(productId, cancellationToken);
        var form = new ProductVariantFormViewModel
        {
            SpecOptionIds = variantValues
                .Where(value => value.ProductVariantId == variant.Id)
                .Select(value => value.SpecOptionId)
                .ToList(),
            ProductId = product.Id,
            ProductName = product.Name,
            VariantId = variant.Id,
            SKU = variant.SKU,
            VariantLabel = variant.VariantLabel,
            Price = (long)variant.Price,
            CurrentStock = variant.StockQuantity,
            IsActive = variant.IsActive
        };
        await PopulateVariantSpecsAsync(form, cancellationToken);
        return form;
    }

    public async Task PopulateVariantSpecsAsync(ProductVariantFormViewModel form, CancellationToken cancellationToken = default)
    {
        var product = await _repository.GetByIdAsync(form.ProductId, cancellationToken);
        if (product is null)
        {
            return;
        }

        var selected = form.SpecOptionIds.ToHashSet();
        var attributes = await _specRepository.GetAttributesAsync(product.CategoryId, cancellationToken);
        form.SpecGroups = attributes
            .Where(attribute => attribute.Scope == SpecScope.Variant)
            .Select(attribute => ToEditorGroup(attribute, selected))
            .ToList();
    }

    public async Task CreateVariantAsync(ProductVariantFormViewModel form, CancellationToken cancellationToken = default)
    {
        var product = await _repository.GetByIdAsync(form.ProductId, cancellationToken)
            ?? throw new KeyNotFoundException();
        var sku = NormalizeSku(form.SKU);
        if (await _repository.IsSkuTakenAsync(sku, null, cancellationToken))
        {
            throw new InvalidOperationException($"Mã SKU {sku} đã tồn tại.");
        }

        var specOptions = await ValidateSpecSelectionAsync(product.CategoryId, SpecScope.Variant, form.SpecOptionIds, cancellationToken);
        var now = DateTime.UtcNow;
        var variant = new ProductVariant
        {
            ProductId = product.Id,
            SKU = sku,
            VariantLabel = NormalizeOptional(form.VariantLabel) ?? LabelFromSpecs(specOptions),
            Price = form.Price,
            StockQuantity = form.InitialStock,
            IsActive = form.IsActive
        };

        if (form.InitialStock > 0)
        {
            variant.InventoryTransactions.Add(new InventoryTransaction
            {
                Type = InventoryTransactionType.Import,
                Quantity = form.InitialStock,
                Note = "Tồn kho ban đầu khi tạo phiên bản",
                CreatedAt = now
            });
        }

        foreach (var option in specOptions)
        {
            variant.SpecValues.Add(new VariantSpecValue { SpecOptionId = option.Id });
        }

        product.ProductVariants.Add(variant);
        product.UpdatedAt = now;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateVariantAsync(ProductVariantFormViewModel form, CancellationToken cancellationToken = default)
    {
        var variant = await _repository.GetVariantAsync(form.VariantId ?? 0, cancellationToken);
        if (variant is null || variant.ProductId != form.ProductId)
        {
            throw new KeyNotFoundException();
        }

        var sku = NormalizeSku(form.SKU);
        if (await _repository.IsSkuTakenAsync(sku, variant.Id, cancellationToken))
        {
            throw new InvalidOperationException($"Mã SKU {sku} đã tồn tại.");
        }

        var specOptions = await ValidateSpecSelectionAsync(variant.Product!.CategoryId, SpecScope.Variant, form.SpecOptionIds, cancellationToken);
        await _specRepository.ReplaceVariantValuesAsync(variant.Id, specOptions.Select(option => option.Id).ToList(), cancellationToken);

        variant.SKU = sku;
        variant.VariantLabel = NormalizeOptional(form.VariantLabel) ?? LabelFromSpecs(specOptions);
        variant.Price = form.Price;
        variant.IsActive = form.IsActive;
        variant.Product!.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> DeleteVariantAsync(int variantId, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var variant = await _repository.GetVariantAsync(variantId, token)
                ?? throw new KeyNotFoundException();
            if (await _repository.HasOrderHistoryAsync([variant.Id], token))
            {
                throw new InvalidOperationException(
                    $"Phiên bản {variant.SKU} đã có trong đơn hàng nên không thể xóa. Hãy tắt \"Đang bán\" cho phiên bản này.");
            }

            await _repository.RemoveCartItemsAsync([variant.Id], token);
            variant.Product!.UpdatedAt = DateTime.UtcNow;
            _repository.RemoveVariant(variant);
            return variant.ProductId;
        }, cancellationToken);
    }

    public async Task<int> AdjustStockAsync(StockAdjustmentViewModel adjustment, CancellationToken cancellationToken = default)
    {
        if (adjustment.Type is not (InventoryTransactionType.Import
            or InventoryTransactionType.AdjustmentIncrease
            or InventoryTransactionType.AdjustmentDecrease))
        {
            throw new InvalidOperationException("Loại giao dịch kho không hợp lệ.");
        }

        return await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var variant = await _repository.GetVariantAsync(adjustment.VariantId ?? 0, token)
                ?? throw new KeyNotFoundException();
            var delta = adjustment.Type == InventoryTransactionType.AdjustmentDecrease
                ? -adjustment.Quantity
                : adjustment.Quantity;

            if (variant.StockQuantity + delta < 0)
            {
                throw new InvalidOperationException(
                    $"Không thể giảm {adjustment.Quantity} vì phiên bản {variant.SKU} chỉ còn {variant.StockQuantity} trong kho.");
            }

            var now = DateTime.UtcNow;
            variant.StockQuantity += delta;
            variant.InventoryTransactions.Add(new InventoryTransaction
            {
                Type = adjustment.Type,
                Quantity = delta,
                Note = NormalizeOptional(adjustment.Note) ?? DefaultInventoryNote(adjustment.Type),
                CreatedAt = now
            });
            variant.Product!.UpdatedAt = now;
            return variant.ProductId;
        }, cancellationToken);
    }

    #endregion

    #region Images

    public async Task UploadImagesAsync(
        int productId,
        IReadOnlyList<IFormFile> images,
        CancellationToken cancellationToken = default)
    {
        if (images.Count == 0)
        {
            throw new InvalidOperationException("Vui lòng chọn ít nhất một ảnh.");
        }

        var product = await _repository.GetWithDetailsAsync(productId, cancellationToken)
            ?? throw new KeyNotFoundException();
        await ValidateImagesAsync(images, cancellationToken);

        var savedUrls = await SaveImagesAsync(images, cancellationToken);
        try
        {
            var hasPrimary = product.ProductImages.Any(image => image.IsPrimary);
            var nextSortOrder = product.ProductImages.Count == 0 ? 1 : product.ProductImages.Max(image => image.SortOrder) + 1;
            foreach (var url in savedUrls)
            {
                product.ProductImages.Add(new ProductImage
                {
                    ImageUrl = url,
                    IsPrimary = !hasPrimary,
                    SortOrder = nextSortOrder++
                });
                hasPrimary = true;
            }

            product.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            DeleteFiles(savedUrls);
            throw;
        }
    }

    public async Task<int> SetPrimaryImageAsync(int imageId, CancellationToken cancellationToken = default)
    {
        var image = await _repository.GetImageAsync(imageId, cancellationToken)
            ?? throw new KeyNotFoundException();
        var product = await _repository.GetWithDetailsAsync(image.ProductId, cancellationToken)
            ?? throw new KeyNotFoundException();

        foreach (var item in product.ProductImages)
        {
            item.IsPrimary = item.Id == imageId;
        }

        product.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return product.Id;
    }

    public async Task<int> DeleteImageAsync(int imageId, CancellationToken cancellationToken = default)
    {
        var image = await _repository.GetImageAsync(imageId, cancellationToken)
            ?? throw new KeyNotFoundException();
        var product = await _repository.GetWithDetailsAsync(image.ProductId, cancellationToken)
            ?? throw new KeyNotFoundException();

        if (image.IsPrimary)
        {
            var nextPrimary = product.ProductImages
                .Where(item => item.Id != image.Id)
                .OrderBy(item => item.SortOrder)
                .FirstOrDefault();
            if (nextPrimary is not null)
            {
                nextPrimary.IsPrimary = true;
            }
        }

        var imageUrl = image.ImageUrl;
        _repository.RemoveImage(image);
        product.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _imageStorage.Delete(imageUrl);
        return product.Id;
    }

    #endregion

    #region Helpers

    private async Task<IReadOnlyList<ManagementOptionViewModel>> GetCategoryOptionsAsync(CancellationToken cancellationToken)
    {
        var categories = await _repository.GetCategoriesAsync(cancellationToken);
        return categories.Select(category => new ManagementOptionViewModel
        {
            Id = category.Id,
            Name = category.Name,
            IsActive = category.IsActive
        }).ToList();
    }

    private async Task<IReadOnlyList<ManagementOptionViewModel>> GetBrandOptionsAsync(CancellationToken cancellationToken)
    {
        var brands = await _repository.GetBrandsAsync(cancellationToken);
        return brands.Select(brand => new ManagementOptionViewModel
        {
            Id = brand.Id,
            Name = brand.Name,
            IsActive = brand.IsActive
        }).ToList();
    }

    private async Task EnsureCategoryAndBrandAsync(ProductFormViewModel form, CancellationToken cancellationToken)
    {
        if (!form.CategoryId.HasValue || !await _repository.CategoryExistsAsync(form.CategoryId.Value, cancellationToken))
        {
            throw new InvalidOperationException("Danh mục đã chọn không tồn tại.");
        }

        if (!form.BrandId.HasValue || !await _repository.BrandExistsAsync(form.BrandId.Value, cancellationToken))
        {
            throw new InvalidOperationException("Thương hiệu đã chọn không tồn tại.");
        }
    }

    private async Task ValidateImagesAsync(IReadOnlyList<IFormFile> images, CancellationToken cancellationToken)
    {
        if (images.Count > MaxImagesPerUpload)
        {
            throw new InvalidOperationException($"Chỉ được tải lên tối đa {MaxImagesPerUpload} ảnh mỗi lần.");
        }

        foreach (var image in images)
        {
            var error = await _imageStorage.ValidateAsync(image, cancellationToken);
            if (error is not null)
            {
                throw new InvalidOperationException(error);
            }
        }
    }

    private async Task<List<string>> SaveImagesAsync(IReadOnlyList<IFormFile> images, CancellationToken cancellationToken)
    {
        var savedUrls = new List<string>(images.Count);
        try
        {
            foreach (var image in images)
            {
                savedUrls.Add(await _imageStorage.SaveAsync(image, cancellationToken));
            }
        }
        catch
        {
            DeleteFiles(savedUrls);
            throw;
        }

        return savedUrls;
    }

    private void DeleteFiles(IEnumerable<string> urls)
    {
        foreach (var url in urls)
        {
            _imageStorage.Delete(url);
        }
    }

    /// <summary>Why an otherwise active product is not visible in the shop (inactive category and/or brand).</summary>
    private static string? GetHiddenReason(Category? category, Brand? brand)
    {
        var reasons = new List<string>(2);
        if (category is { IsActive: false })
        {
            reasons.Add($"danh mục \"{category.Name}\"");
        }

        if (brand is { IsActive: false })
        {
            reasons.Add($"thương hiệu \"{brand.Name}\"");
        }

        return reasons.Count == 0 ? null : $"Đang ẩn khỏi cửa hàng vì {string.Join(" và ", reasons)} ngừng hoạt động.";
    }

    /// <summary>
    /// Checks that every option exists and belongs to the product's category with the expected scope;
    /// a variant may have at most one value per attribute. Returns the options ordered like the attributes.
    /// </summary>
    private async Task<IReadOnlyList<SpecOption>> ValidateSpecSelectionAsync(
        int categoryId,
        SpecScope scope,
        IEnumerable<int> optionIds,
        CancellationToken cancellationToken)
    {
        var ids = optionIds.Where(id => id > 0).Distinct().ToList();
        var options = await _specRepository.GetOptionsAsync(ids, cancellationToken);
        if (options.Count != ids.Count
            || options.Any(option => option.Attribute!.CategoryId != categoryId || option.Attribute.Scope != scope))
        {
            throw new InvalidOperationException("Thông số đã chọn không thuộc danh mục của sản phẩm. Vui lòng tải lại trang và chọn lại.");
        }

        if (scope == SpecScope.Variant && options.GroupBy(option => option.SpecAttributeId).Any(group => group.Count() > 1))
        {
            throw new InvalidOperationException("Mỗi thông số của phiên bản chỉ được chọn một giá trị.");
        }

        return options
            .OrderBy(option => option.Attribute!.SortOrder)
            .ThenBy(option => option.SortOrder)
            .ToList();
    }

    private static string? LabelFromSpecs(IReadOnlyList<SpecOption> options) =>
        options.Count == 0 ? null : string.Join(" - ", options.Select(option => option.Value));

    private static SpecEditorGroupViewModel ToEditorGroup(SpecAttribute attribute, IReadOnlySet<int> selected) => new()
    {
        AttributeId = attribute.Id,
        Name = attribute.Name,
        Hint = attribute.Hint,
        Options = attribute.Options.Select(option => new SpecEditorOptionViewModel
        {
            Id = option.Id,
            Value = option.Value,
            Hint = option.Hint,
            Selected = selected.Contains(option.Id)
        }).ToList()
    };

    private static string NormalizeSku(string sku) => sku.Trim().ToUpperInvariant();

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string DefaultInventoryNote(InventoryTransactionType type) => type switch
    {
        InventoryTransactionType.Import => "Nhập hàng",
        InventoryTransactionType.AdjustmentIncrease => "Điều chỉnh tăng tồn kho",
        InventoryTransactionType.AdjustmentDecrease => "Điều chỉnh giảm tồn kho",
        _ => "Giao dịch kho"
    };

    #endregion
}
