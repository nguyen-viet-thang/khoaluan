using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PopPop.Data.Entities;
using PopPop.Services.Implementations;
using PopPop.Services.Interfaces;

namespace PopPop.Areas.Identity.Pages.Account;

/// <summary>
/// Customer sign-in with email + password (accounts registered by phone number can still use their phone).
/// An email account that is not verified yet cannot sign in: a new verification email is sent instead.
/// </summary>
[AllowAnonymous]
public class LoginModel : PageModel
{
    private const string InvalidLoginMessage = "Email/số điện thoại hoặc mật khẩu không chính xác.";

    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly IAccountEmailService _accountEmailService;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<LoginModel> _logger;

    public LoginModel(
        SignInManager<User> signInManager,
        UserManager<User> userManager,
        IAccountEmailService accountEmailService,
        IWebHostEnvironment environment,
        ILogger<LoginModel> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _accountEmailService = accountEmailService;
        _environment = environment;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = default!;

    public string? ReturnUrl { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    /// <summary>Set when the account exists but its email is not verified yet.</summary>
    public string? UnconfirmedEmail { get; private set; }
    public bool ConfirmationEmailDelivered { get; private set; }
    /// <summary>Development only, when no SMTP server is configured.</summary>
    public string? DevConfirmationUrl { get; private set; }

    public class InputModel
    {
        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [Display(Name = "Email")]
        public string Login { get; set; } = default!;

        [Required(ErrorMessage = "Mật khẩu không được để trống.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = default!;

        public bool RememberMe { get; set; }
    }

    public void OnGet(string? returnUrl = null)
    {
        if (!string.IsNullOrEmpty(ErrorMessage))
        {
            ModelState.AddModelError(string.Empty, ErrorMessage);
        }

        ReturnUrl = returnUrl ?? Url.Content("~/");
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        ReturnUrl = returnUrl;
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var userName = ToUserName(Input.Login);
        if (userName is null)
        {
            ModelState.AddModelError("Input.Login", "Vui lòng nhập email hợp lệ (hoặc số điện thoại nếu tài khoản cũ đăng ký bằng số điện thoại).");
            return Page();
        }

        var result = await _signInManager.PasswordSignInAsync(userName, Input.Password, Input.RememberMe, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            _logger.LogInformation("User logged in.");
            return LocalRedirect(returnUrl);
        }

        if (result.RequiresTwoFactor)
        {
            return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, Input.RememberMe });
        }

        if (result.IsLockedOut)
        {
            _logger.LogWarning("User account locked out.");
            return RedirectToPage("./Lockout");
        }

        if (result.IsNotAllowed)
        {
            // Email not verified. Only say so (and re-send the link) when the password is right,
            // so the form cannot be used to probe which emails are registered.
            var user = await _userManager.FindByNameAsync(userName);
            if (user is not null && !string.IsNullOrEmpty(user.Email) && !user.EmailConfirmed
                && await _userManager.CheckPasswordAsync(user, Input.Password))
            {
                UnconfirmedEmail = user.Email;
                string url;
                (ConfirmationEmailDelivered, url) = await EmailConfirmationLink.SendAsync(this, _userManager, _accountEmailService, user);
                if (!ConfirmationEmailDelivered && _environment.IsDevelopment())
                {
                    DevConfirmationUrl = url;
                }

                return Page();
            }
        }

        ModelState.AddModelError(string.Empty, InvalidLoginMessage);
        return Page();
    }

    /// <summary>Email → lower-cased email; otherwise a normalised phone number (older accounts); null when neither.</summary>
    private static string? ToUserName(string login)
    {
        var value = login.Trim();
        if (value.Contains('@'))
        {
            return System.Text.RegularExpressions.Regex.IsMatch(value, EmailConfirmationLink.EmailAddressPattern)
                ? EmailConfirmationLink.Normalize(value)
                : null;
        }

        var phone = PhoneNumberFormatter.Normalize(value);
        return PhoneNumberFormatter.IsValid(phone) ? phone : null;
    }
}
