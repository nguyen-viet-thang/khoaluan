using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PopPop.Areas.Identity.Pages.Account;

/// <summary>"Check your inbox" page shown after registering (and after asking for a new verification email).</summary>
[AllowAnonymous]
public class RegisterConfirmationModel : PageModel
{
    public string? Email { get; private set; }
    public string? ReturnUrl { get; private set; }
    /// <summary>False when the mail could not be handed to the mail server.</summary>
    public bool Delivered { get; private set; } = true;
    /// <summary>Development only, when no SMTP server is configured.</summary>
    public string? DevConfirmationUrl { get; private set; }

    public void OnGet(string? email = null, string? returnUrl = null)
    {
        Email = email;
        ReturnUrl = returnUrl;
        if (TempData["ConfirmationEmailDelivered"] is bool delivered)
        {
            Delivered = delivered;
        }

        DevConfirmationUrl = TempData["DevConfirmationUrl"] as string;
    }
}
