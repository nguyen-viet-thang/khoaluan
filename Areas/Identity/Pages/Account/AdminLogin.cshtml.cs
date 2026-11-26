using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PopPop.Data.Entities;

namespace PopPop.Areas.Identity.Pages.Account;

[AllowAnonymous]
public sealed class AdminLoginModel : PageModel
{
    private const string AdminRole = "Admin";
    private const string StaffRole = "Staff";
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;

    public AdminLoginModel(SignInManager<User> signInManager, UserManager<User> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    public sealed class InputModel
    {
        [Required(ErrorMessage = "Vui lòng nhập Gmail hoặc số điện thoại.")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu không được để trống.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
        var user = await _userManager.GetUserAsync(User);
        if (user is not null)
        {
            var destination = await GetManagementDestinationAsync(user);
            if (destination is not null)
            {
                return Redirect(destination);
            }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await FindManagementUserAsync(Input.PhoneNumber);
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Thông tin đăng nhập không hợp lệ hoặc tài khoản không có quyền truy cập khu vực quản trị.");
            return Page();
        }

        var destination = await GetManagementDestinationAsync(user);
        if (destination is null)
        {
            ModelState.AddModelError(string.Empty, "Thông tin đăng nhập không hợp lệ hoặc tài khoản không có quyền truy cập khu vực quản trị.");
            return Page();
        }

        var result = await _signInManager.PasswordSignInAsync(user, Input.Password, Input.RememberMe, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            destination = await GetManagementDestinationAsync(user);
            if (destination is not null)
            {
                return Redirect(destination);
            }

            await _signInManager.SignOutAsync();
            ModelState.AddModelError(string.Empty, "Tài khoản không có quyền truy cập khu vực quản trị.");
            return Page();
        }

        if (result.RequiresTwoFactor)
        {
            return RedirectToPage("./LoginWith2fa", new
            {
                ReturnUrl = destination,
                RememberMe = Input.RememberMe
            });
        }

        if (result.IsLockedOut)
        {
            return RedirectToPage("./Lockout");
        }

        ModelState.AddModelError(string.Empty, "Thông tin đăng nhập không hợp lệ hoặc tài khoản không có quyền truy cập khu vực quản trị.");
        return Page();
    }

    /// <summary>
    /// Staff sign in with their Gmail (their phone number also works); the admin and older staff
    /// accounts use the phone number that is their user name.
    /// </summary>
    private async Task<User?> FindManagementUserAsync(string login)
    {
        var value = login.Trim();
        if (value.Contains('@'))
        {
            var email = EmailConfirmationLink.Normalize(value);
            return await _userManager.FindByNameAsync(email) ?? await _userManager.FindByEmailAsync(email);
        }

        var phone = PopPop.Services.Implementations.PhoneNumberFormatter.Normalize(value);
        if (!PopPop.Services.Implementations.PhoneNumberFormatter.IsValid(phone))
        {
            return null;
        }

        var byUserName = await _userManager.FindByNameAsync(phone!);
        if (byUserName is not null)
        {
            return byUserName;
        }

        var samePhone = await _userManager.Users.Where(user => user.PhoneNumber == phone).ToListAsync();
        foreach (var candidate in samePhone)
        {
            if (await GetManagementDestinationAsync(candidate) is not null)
            {
                return candidate;
            }
        }

        return null;
    }

    private async Task<string?> GetManagementDestinationAsync(User user)
    {
        if (await _userManager.IsInRoleAsync(user, AdminRole))
        {
            return "/Admin";
        }

        return await _userManager.IsInRoleAsync(user, StaffRole) ? "/Staff" : null;
    }
}
