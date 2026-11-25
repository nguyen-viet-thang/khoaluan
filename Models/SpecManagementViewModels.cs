using System.ComponentModel.DataAnnotations;
using PopPop.Data.Entities;

namespace PopPop.Models;

public sealed class SpecManagementViewModel
{
    public IReadOnlyList<ManagementOptionViewModel> Categories { get; init; } = [];
    public int? CategoryId { get; init; }
    public string? CategoryName { get; init; }
    /// <summary>Products in the selected category (any status).</summary>
    public int CategoryProductCount { get; init; }
    public IReadOnlyList<SpecAttributeItemViewModel> Attributes { get; init; } = [];
}

public sealed class SpecAttributeItemViewModel
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Hint { get; init; }
    public SpecScope Scope { get; init; }
    /// <summary>Total assignments of all its options (products or variants).</summary>
    public int UsageCount { get; init; }
    public IReadOnlyList<SpecOptionItemViewModel> Options { get; init; } = [];
}

public sealed class SpecOptionItemViewModel
{
    public int Id { get; init; }
    public string Value { get; init; } = string.Empty;
    public string? Hint { get; init; }
    public int UsageCount { get; init; }
}

public sealed class SpecAttributeFormViewModel
{
    public int? Id { get; set; }
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên thông số.")]
    [StringLength(100, ErrorMessage = "Tên thông số không được vượt quá 100 ký tự.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(300, ErrorMessage = "Giải thích không được vượt quá 300 ký tự.")]
    public string? Hint { get; set; }

    [EnumDataType(typeof(SpecScope), ErrorMessage = "Phạm vi thông số không hợp lệ.")]
    public SpecScope Scope { get; set; } = SpecScope.Product;
}

public sealed class SpecOptionFormViewModel
{
    public int? Id { get; set; }
    public int AttributeId { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập giá trị.")]
    [StringLength(100, ErrorMessage = "Giá trị không được vượt quá 100 ký tự.")]
    public string Value { get; set; } = string.Empty;

    [StringLength(300, ErrorMessage = "Giải thích không được vượt quá 300 ký tự.")]
    public string? Hint { get; set; }
}
