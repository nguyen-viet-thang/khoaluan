using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PopPop.Data.Entities;
using PopPop.Services.Interfaces;

namespace PopPop.Areas.Identity.Pages.Account;

/// <summary>Sends a new verification link. The answer is the same whether or not the email exists.</summary>
[AllowAnonymous]
public class ResendEmailConfirmationModel : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly IAccountEmailService _accountEmailService;
    private readonly IWebHostEnvironment _environment;

    public ResendEmailConfirmationModel(UserManager<User> userManager, IAccountEmailService accountEmailService, IWebHostEnvironment environment)
    {
        _userManager = userManager;
        _accountEmailService = accountEmailService;
        _environment = environment;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required(ErrorMessage = "Vui lòng nhập email.")]
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
        var user = await _userManager.FindByEmailAsync(email);
        var delivered = true;
        if (user is not null && !user.EmailConfirmed)
        {
            string url;
            (delivered, url) = await EmailConfirmationLink.SendAsync(this, _userManager, _accountEmailService, user);
            if (!delivered && _environment.IsDevelopment())
            {
                TempData["DevConfirmationUrl"] = url;
            }
        }

        TempData["ConfirmationEmailDelivered"] = delivered;
        return RedirectToPage("./RegisterConfirmation", new { email });
    }
}
