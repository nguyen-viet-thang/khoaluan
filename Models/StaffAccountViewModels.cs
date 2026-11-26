using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace PopPop.Models;

public static class StaffAccountRules
{
    /// <summary>Staff sign in with their own Gmail address.</summary>
    public const string GmailPattern = @"^\s*[A-Za-z0-9._%+-]+@(gmail|googlemail)\.com\s*$";
    public const string GmailMessage = "Vui lòng nhập địa chỉ Gmail hợp lệ của nhân viên (ví dụ: ten.nhanvien@gmail.com).";
}

public sealed class CreateStaffAccountViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
    [StringLength(256, ErrorMessage = "Họ và tên không được vượt quá 256 ký tự.")]
    [Display(Name = "Họ và tên")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập Gmail của nhân viên.")]
    [StringLength(256, ErrorMessage = "Gmail không được vượt quá 256 ký tự.")]
    [RegularExpression(StaffAccountRules.GmailPattern, ErrorMessage = StaffAccountRules.GmailMessage)]
    [Display(Name = "Gmail của nhân viên (dùng để đăng nhập)")]
    public string Email { get; set; } = string.Empty;

    /// <summary>Also the initial password: the staff member must change it at the first sign-in.</summary>
    [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
    [Display(Name = "Số điện thoại (là mật khẩu ban đầu)")]
    public string PhoneNumber { get; set; } = string.Empty;
}

public sealed class EditStaffAccountViewModel
{
    [ValidateNever]
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
    [StringLength(256, ErrorMessage = "Họ và tên không được vượt quá 256 ký tự.")]
    [Display(Name = "Họ và tên")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập Gmail của nhân viên.")]
    [StringLength(256, ErrorMessage = "Gmail không được vượt quá 256 ký tự.")]
    [RegularExpression(StaffAccountRules.GmailPattern, ErrorMessage = StaffAccountRules.GmailMessage)]
    [Display(Name = "Gmail của nhân viên (dùng để đăng nhập)")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
    [Display(Name = "Số điện thoại")]
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
    /// <summary>Null for an older account that still signs in with its phone number only.</summary>
    public string? Email { get; init; }
    public string PhoneNumber { get; init; } = string.Empty;
    public bool MustChangePassword { get; init; }
    public bool IsLocked { get; init; }
    public string? LockReason { get; init; }
    public DateTime CreatedAt { get; init; }
}
