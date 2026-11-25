using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace PopPop.Models;

public sealed class CreateStaffAccountViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
    [StringLength(256, ErrorMessage = "Họ và tên không được vượt quá 256 ký tự.")]
    [Display(Name = "Họ và tên")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
    [Display(Name = "Số điện thoại (dùng để đăng nhập)")]
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>Optional: left empty, the system generates one. The staff member must change it at the first sign-in.</summary>
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu ban đầu cần từ {2} đến {1} ký tự.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu ban đầu (bỏ trống để hệ thống tự tạo)")]
    public string? InitialPassword { get; set; }
}

public sealed class EditStaffAccountViewModel
{
    [ValidateNever]
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
    [StringLength(256, ErrorMessage = "Họ và tên không được vượt quá 256 ký tự.")]
    [Display(Name = "Họ và tên")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
    [Display(Name = "Số điện thoại (dùng để đăng nhập)")]
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>Still using the password set by the admin (has not signed in and changed it yet).</summary>
    public bool MustChangePassword { get; set; }
    public bool IsLocked { get; set; }
    public string? LockReason { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class StaffAccountListViewModel
{
    public IReadOnlyList<StaffAccountItemViewModel> Accounts { get; init; } = [];
    public string? Search { get; init; }
    public string? Status { get; init; }
}

public sealed class StaffAccountItemViewModel
{
    public string Id { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public bool MustChangePassword { get; init; }
    public bool IsLocked { get; init; }
    public string? LockReason { get; init; }
    public DateTime CreatedAt { get; init; }
}
