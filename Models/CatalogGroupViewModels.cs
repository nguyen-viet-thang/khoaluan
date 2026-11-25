using System.ComponentModel.DataAnnotations;

namespace PopPop.Models;

public sealed class CatalogGroupListViewModel
{
    public IReadOnlyList<CatalogGroupItemViewModel> Items { get; init; } = [];
    public string? Search { get; init; }
    public string? Status { get; init; }
}

public sealed class CatalogGroupItemViewModel
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsActive { get; init; }
    public int ProductCount { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public sealed class CatalogGroupFormViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên.")]
    [StringLength(256, ErrorMessage = "Tên không được vượt quá 256 ký tự.")]
    [Display(Name = "Tên")]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự.")]
    [Display(Name = "Mô tả")]
    public string? Description { get; set; }

    [Display(Name = "Đang hoạt động")]
    public bool IsActive { get; set; } = true;

    public int ProductCount { get; set; }
}
