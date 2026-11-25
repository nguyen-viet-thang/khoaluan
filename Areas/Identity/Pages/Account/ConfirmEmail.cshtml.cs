using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using PopPop.Data.Entities;

namespace PopPop.Areas.Identity.Pages.Account;

/// <summary>Target of the link in the verification email. Does not sign the user in: they log in with their password.</summary>
[AllowAnonymous]
public class ConfirmEmailModel : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly ILogger<ConfirmEmailModel> _logger;

    public ConfirmEmailModel(UserManager<User> userManager, ILogger<ConfirmEmailModel> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public bool Succeeded { get; private set; }
    public bool AlreadyConfirmed { get; private set; }
    public string? Email { get; private set; }

    public async Task<IActionResult> OnGetAsync(string? userId, string? code)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(code))
        {
            return RedirectToPage("./Login");
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null || string.IsNullOrEmpty(user.Email))
        {
            return Page();
        }

        Email = user.Email;
        if (user.EmailConfirmed)
        {
            AlreadyConfirmed = Succeeded = true;
            return Page();
        }

        string token;
        try
        {
            token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        }
        catch (FormatException)
        {
            return Page();
        }

        var result = await _userManager.ConfirmEmailAsync(user, token);
        Succeeded = result.Succeeded;
        if (Succeeded)
        {
            _logger.LogInformation("User {UserId} confirmed their email.", user.Id);
        }

        return Page();
    }
}
