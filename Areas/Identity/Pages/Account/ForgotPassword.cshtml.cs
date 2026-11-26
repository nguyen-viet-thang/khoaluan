using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using PopPop.Data.Entities;
using PopPop.Services.Interfaces;

namespace PopPop.Areas.Identity.Pages.Account;

/// <summary>
/// Step 1 of "forgot password": mails a one-time link to /Identity/Account/ResetPassword.
/// The answer is the same whether or not the email belongs to an account.
/// </summary>
[AllowAnonymous]
public class ForgotPasswordModel : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly IAccountEmailService _accountEmailService;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ForgotPasswordModel> _logger;

    public ForgotPasswordModel(
        UserManager<User> userManager,
        IAccountEmailService accountEmailService,
        IWebHostEnvironment environment,
        ILogger<ForgotPasswordModel> logger)
    {
        _userManager = userManager;
        _accountEmailService = accountEmailService;
        _environment = environment;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [StringLength(256, ErrorMessage = "Email tối đa {1} ký tự.")]
        [RegularExpression(EmailConfirmationLink.EmailAddressPattern, ErrorMessage = "Email không hợp lệ.")]
        public string Email { get; set; } = string.Empty;
    }

    public void OnGet(string? email = null) => Input.Email = email ?? string.Empty;

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var email = EmailConfirmationLink.Normalize(Input.Email);
        var user = await FindByEmailAsync(email);
        var delivered = true;
        if (user is not null)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var url = Url.Page(
                "/Account/ResetPassword",
                pageHandler: null,
                values: new { area = "Identity", userId = user.Id, code },
                protocol: Request.Scheme)!;

            delivered = await _accountEmailService.SendPasswordResetAsync(user.Email!, user.FullName, url, HttpContext.RequestAborted);
            if (!delivered && _environment.IsDevelopment())
            {
                TempData["DevResetUrl"] = url;
            }
        }

        TempData["ResetEmailDelivered"] = delivered;
        return RedirectToPage("./ForgotPasswordConfirmation", new { email });
    }

    private async Task<User?> FindByEmailAsync(string email)
    {
        try
        {
            // Customers: UserName = email. Staff: UserName = Gmail as well (older accounts may only have Email set).
            return await _userManager.FindByNameAsync(email) ?? await _userManager.FindByEmailAsync(email);
        }
        catch (InvalidOperationException exception)
        {
            // Emails are not unique at the Identity level; never guess which account to reset.
            _logger.LogWarning(exception, "Password reset skipped: several accounts share the email {Email}.", email);
            return null;
        }
    }
}
