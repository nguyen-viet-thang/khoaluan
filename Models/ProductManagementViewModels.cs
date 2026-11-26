using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using PopPop.Data.Entities.Enums;

namespace PopPop.Models;

public sealed class ManagementOptionViewModel
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool IsActive { get; init; }
}

public sealed class ProductManagementListViewModel
{
    public IReadOnlyList<ProductManagementItemViewModel> Products { get; init; } = [];
    public IReadOnlyList<ManagementOptionViewModel> Categories { get; init; } = [];
    public IReadOnlyList<ManagementOptionViewModel> Brands { get; init; } = [];
    public string? Search { get; init; }
    public int? CategoryId { get; init; }
    public int? BrandId { get; init; }
    public string? Status { get; init; }
}

public sealed class ProductManagementItemViewModel
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public string BrandName { get; init; } = string.Empty;
    public int VariantCount { get; init; }
    public int TotalStock { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public bool IsActive { get; init; }
    /// <summary>Set when the product is active but hidden because its category or brand is inactive.</summary>
    public string? HiddenReason { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public sealed class ProductFormViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm.")]
    [StringLength(256, ErrorMessage = "Tên sản phẩm không được vượt quá 256 ký tự.")]
    [Display(Name = "Tên sản phẩm")]
    public string Name { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Mô tả không được vượt quá 2000 ký tự.")]
    [Display(Name = "Mô tả")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn danh mục.")]
    [Display(Name = "Danh mục")]
    public int? CategoryId { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn thương hiệu.")]
    [Display(Name = "Thương hiệu")]
    public int? BrandId { get; set; }

    [Display(Name = "Đang bán")]
    public bool IsActive { get; set; } = true;

    [ValidateNever]
    public IReadOnlyList<ManagementOptionViewModel> Categories { get; set; } = [];
    [ValidateNever]
    public IReadOnlyList<ManagementOptionViewModel> Brands { get; set; } = [];
}

public sealed class ProductEditViewModel
{
    public ProductFormViewModel Form { get; init; } = new();
    /// <summary>Set when the product is hidden from the shop because its category or brand is inactive.</summary>
    public string? HiddenReason { get; init; }
    public IReadOnlyList<ProductVariantItemViewModel> Variants { get; init; } = [];
    /// <summary>Product-level specifications of the product's category, with the current selection.</summary>
    public IReadOnlyList<SpecEditorGroupViewModel> ProductSpecs { get; init; } = [];
    /// <summary>Names of the variant-level specifications (set on each variant).</summary>
    public IReadOnlyList<string> VariantSpecNames { get; init; } = [];
    public IReadOnlyList<ProductImageItemViewModel> Images { get; init; } = [];
    public IReadOnlyList<InventoryHistoryItemViewModel> InventoryHistory { get; init; } = [];
    public StockAdjustmentViewModel StockAdjustment { get; init; } = new();
}

public sealed class ProductVariantItemViewModel
{
    public int Id { get; init; }
    public string SKU { get; init; } = string.Empty;
    public string? VariantLabel { get; init; }
    public decimal Price { get; init; }
    public int StockQuantity { get; init; }
    public bool IsActive { get; init; }
    /// <summary>Variant specifications, e.g. "4U · G5"; null when none are set.</summary>
    public string? SpecSummary { get; init; }
    /// <summary>Some variant-level specification of the category is not set (the variant will not match that filter).</summary>
    public bool IsMissingSpecs { get; init; }
}

public sealed class SpecEditorGroupViewModel
{
    public int AttributeId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Hint { get; init; }
    public IReadOnlyList<SpecEditorOptionViewModel> Options { get; init; } = [];
    public int? SelectedOptionId => Options.FirstOrDefault(option => option.Selected)?.Id;
}

public sealed class SpecEditorOptionViewModel
{
    public int Id { get; init; }
    public string Value { get; init; } = string.Empty;
    public string? Hint { get; init; }
    public bool Selected { get; init; }
}

public sealed class ProductImageItemViewModel
{
    public int Id { get; init; }
    public string ImageUrl { get; init; } = string.Empty;
    public bool IsPrimary { get; init; }
}

public sealed class InventoryHistoryItemViewModel
{
    public DateTime CreatedAt { get; init; }
    public string SKU { get; init; } = string.Empty;
    public InventoryTransactionType Type { get; init; }
    public int Quantity { get; init; }
    public string? Note { get; init; }
}

public sealed class ProductVariantFormViewModel
{
    public int ProductId { get; set; }
    public int? VariantId { get; set; }
    [ValidateNever]
    public string ProductName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mã SKU.")]
    [StringLength(100, ErrorMessage = "Mã SKU không được vượt quá 100 ký tự.")]
    [RegularExpression(@"^[A-Za-z0-9\-_.]+$", ErrorMessage = "SKU chỉ gồm chữ không dấu, số và các ký tự - _ .")]
    [Display(Name = "Mã SKU")]
    public string SKU { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Tên phiên bản không được vượt quá 100 ký tự.")]
    [Display(Name = "Tên phiên bản (để trống sẽ ghép từ thông số, VD: 4U - G5)")]
    public string? VariantLabel { get; set; }

    [Range(0, 1_000_000_000, ErrorMessage = "Giá bán phải từ 0 đến 1.000.000.000 ₫.")]
    [Display(Name = "Giá bán (₫)")]
    public long Price { get; set; }

    [Range(0, 100_000, ErrorMessage = "Tồn kho ban đầu phải từ 0 đến 100.000.")]
    [Display(Name = "Tồn kho ban đầu")]
    public int InitialStock { get; set; }

    public int CurrentStock { get; set; }

    [Display(Name = "Đang bán")]
    public bool IsActive { get; set; } = true;

    /// <summary>One option per variant-level attribute (posted from the selects).</summary>
    public List<int> SpecOptionIds { get; set; } = [];

    [ValidateNever]
    public IReadOnlyList<SpecEditorGroupViewModel> SpecGroups { get; set; } = [];
}

public sealed class StockAdjustmentViewModel
{
    [Required(ErrorMessage = "Vui lòng chọn phiên bản.")]
    public int? VariantId { get; set; }

    [Range(1, 100_000, ErrorMessage = "Số lượng phải từ 1 đến 100.000.")]
    public int Quantity { get; set; } = 1;

    [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự.")]
    public string? Note { get; set; }
}
