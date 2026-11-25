using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PopPop.Data.Entities;
using PopPop.Services.Interfaces;

namespace PopPop.Areas.Identity.Pages.Account;

/// <summary>
/// Customer sign-up: full name, email (= login) and password. A verification link is emailed; the account
/// can only sign in once the email is confirmed.
/// </summary>
[AllowAnonymous]
public class RegisterModel : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly IAccountEmailService _accountEmailService;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<RegisterModel> _logger;

    public RegisterModel(
        UserManager<User> userManager,
        IAccountEmailService accountEmailService,
        IWebHostEnvironment environment,
        ILogger<RegisterModel> logger)
    {
        _userManager = userManager;
        _accountEmailService = accountEmailService;
        _environment = environment;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = default!;

    public string? ReturnUrl { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ và tên cần từ {2} đến {1} ký tự.")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = default!;

        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [StringLength(256, ErrorMessage = "Email tối đa {1} ký tự.")]
        [RegularExpression(EmailConfirmationLink.EmailAddressPattern, ErrorMessage = "Email không hợp lệ.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = default!;

        [Required(ErrorMessage = "Mật khẩu không được để trống.")]
        [StringLength(100, ErrorMessage = "Mật khẩu phải có từ {2} đến {1} ký tự.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = default!;

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        public string? ConfirmPassword { get; set; }
    }

    public void OnGet(string? returnUrl = null) => ReturnUrl = returnUrl;

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var email = EmailConfirmationLink.Normalize(Input.Email);
        var existing = await _userManager.FindByNameAsync(email) ?? await _userManager.FindByEmailAsync(email);
        if (existing is not null)
        {
            ModelState.AddModelError("Input.Email", existing.EmailConfirmed || string.IsNullOrEmpty(existing.Email)
                ? "Email này đã được đăng ký. Vui lòng đăng nhập."
                : "Email này đã đăng ký nhưng chưa xác thực. Hãy kiểm tra hộp thư hoặc dùng \"Gửi lại email xác thực\".");
            return Page();
        }

        var user = new User
        {
            UserName = email,
            Email = email,
            EmailConfirmed = false,
            FullName = Input.FullName.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, Input.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                // Duplicate / invalid user name is really about the email field.
                var key = error.Code is nameof(IdentityErrorDescriber.DuplicateUserName) or nameof(IdentityErrorDescriber.InvalidUserName)
                    ? "Input.Email"
                    : string.Empty;
                ModelState.AddModelError(key, error.Description);
            }

            return Page();
        }

        await _userManager.AddToRoleAsync(user, "Customer");
        _logger.LogInformation("Customer {UserId} registered with an email; verification pending.", user.Id);

        var (delivered, url) = await EmailConfirmationLink.SendAsync(this, _userManager, _accountEmailService, user);
        if (!delivered && _environment.IsDevelopment())
        {
            // No SMTP in development: show the link on the confirmation page so the flow can be tested.
            TempData["DevConfirmationUrl"] = url;
        }

        TempData["ConfirmationEmailDelivered"] = delivered;
        return RedirectToPage("./RegisterConfirmation", new { email, returnUrl });
    }
}
