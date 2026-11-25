using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PopPop.Data.Entities;
using PopPop.Services.Implementations;

namespace PopPop.Areas.Identity.Pages.Account.Manage;

/// <summary>
/// Profile: full name and phone number. Email accounts sign in with their (verified) email, so the phone is just
/// contact info (optional); for phone accounts the phone number is also the login and both change together.
/// </summary>
public class IndexModel : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;

    public IndexModel(UserManager<User> userManager, SignInManager<User> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [TempData]
    public string? StatusMessage { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = default!;

    public class InputModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ và tên cần từ {2} đến {1} ký tự.")]
        public string FullName { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }
    }

    /// <summary>Login email (null for phone-number accounts).</summary>
    public string? Email { get; private set; }

    private static bool IsEmailAccount(User user) => !string.IsNullOrEmpty(user.Email);

    private void Load(User user)
    {
        Email = user.Email;
        Input = new InputModel
        {
            FullName = user.FullName ?? string.Empty,
            PhoneNumber = IsEmailAccount(user) ? user.PhoneNumber : user.PhoneNumber ?? user.UserName
        };
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound();
        }

        Load(user);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound();
        }

        Email = user.Email;
        var emailAccount = IsEmailAccount(user);
        var phone = string.IsNullOrWhiteSpace(Input.PhoneNumber) ? null : PhoneNumberFormatter.Normalize(Input.PhoneNumber);
        if (phone is null && !emailAccount)
        {
            ModelState.AddModelError("Input.PhoneNumber", "Vui lòng nhập số điện thoại.");
        }
        else if (phone is not null && !PhoneNumberFormatter.IsValid(phone))
        {
            ModelState.AddModelError("Input.PhoneNumber", PhoneNumberFormatter.InvalidMessage);
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (!emailAccount && !string.Equals(phone, user.UserName, StringComparison.Ordinal))
        {
            // Changing the login: SetUserNameAsync validates uniqueness (DuplicateUserName) before saving.
            var renamed = await _userManager.SetUserNameAsync(user, phone);
            if (!renamed.Succeeded)
            {
                foreach (var error in renamed.Errors)
                {
                    ModelState.AddModelError("Input.PhoneNumber", error.Description);
                }

                return Page();
            }
        }

        user.PhoneNumber = phone;
        user.FullName = Input.FullName.Trim();
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            StatusMessage = "Lỗi: Không thể cập nhật hồ sơ. Vui lòng thử lại.";
            return RedirectToPage();
        }

        await _signInManager.RefreshSignInAsync(user);
        StatusMessage = emailAccount
            ? "Đã cập nhật hồ sơ."
            : "Đã cập nhật hồ sơ. Từ lần sau, hãy đăng nhập bằng số điện thoại " + phone + ".";
        return RedirectToPage();
    }
}
