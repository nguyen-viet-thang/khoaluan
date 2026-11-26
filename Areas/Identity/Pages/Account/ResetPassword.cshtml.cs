using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using PopPop.Data.Entities;

namespace PopPop.Areas.Identity.Pages.Account;

/// <summary>
/// Step 2 of "forgot password": the emailed link (?userId=…&amp;code=…) lets the owner of the mailbox choose a new password.
/// The token is single-use (resetting changes the security stamp) and expires with the default token lifespan (24 h).
/// </summary>
[AllowAnonymous]
public class ResetPasswordModel : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly ILogger<ResetPasswordModel> _logger;

    public ResetPasswordModel(UserManager<User> userManager, ILogger<ResetPasswordModel> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    /// <summary>The link is broken, expired or already used.</summary>
    public bool InvalidLink { get; private set; }

    public string? AccountEmail { get; private set; }

    public class InputModel
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
        [StringLength(100, ErrorMessage = "Mật khẩu phải có từ {2} đến {1} ký tự.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        public string? ConfirmPassword { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(string? userId = null, string? code = null)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(code))
        {
            InvalidLink = true;
            return Page();
        }

        var user = await _userManager.FindByIdAsync(userId);
        var token = DecodeToken(code);
        // Check the token up front so an expired/used link says so before the user types a new password.
        if (user is null || token is null
            || !await _userManager.VerifyUserTokenAsync(user, _userManager.Options.Tokens.PasswordResetTokenProvider,
                UserManager<User>.ResetPasswordTokenPurpose, token))
        {
            InvalidLink = true;
            return Page();
        }

        Input = new InputModel { UserId = userId, Code = code };
        AccountEmail = user.Email ?? user.UserName;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = string.IsNullOrEmpty(Input.UserId) ? null : await _userManager.FindByIdAsync(Input.UserId);
        var token = DecodeToken(Input.Code);
        if (user is null || token is null)
        {
            InvalidLink = true;
            return Page();
        }

        AccountEmail = user.Email ?? user.UserName;
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _userManager.ResetPasswordAsync(user, token, Input.Password);
        if (!result.Succeeded)
        {
            if (result.Errors.Any(error => error.Code == nameof(IdentityErrorDescriber.InvalidToken)))
            {
                InvalidLink = true;
                return Page();
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }

        // The user chose this password themselves, and opening the mailed link proves they own the email.
        user.MustChangePassword = false;
        if (!string.IsNullOrEmpty(user.Email))
        {
            user.EmailConfirmed = true;
        }

        await _userManager.UpdateAsync(user);
        await _userManager.ResetAccessFailedCountAsync(user);
        _logger.LogInformation("User {UserId} reset their password via the emailed link.", user.Id);

        var isManagement = await _userManager.IsInRoleAsync(user, "Admin") || await _userManager.IsInRoleAsync(user, "Staff");
        return RedirectToPage("./ResetPasswordConfirmation", new { management = isManagement });
    }

    private static string? DecodeToken(string? code)
    {
        if (string.IsNullOrEmpty(code))
        {
            return null;
        }

        try
        {
            return Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        }
        catch (FormatException)
        {
            return null;
        }
    }
}
