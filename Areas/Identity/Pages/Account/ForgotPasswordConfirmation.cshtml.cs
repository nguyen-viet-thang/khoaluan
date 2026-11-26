using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PopPop.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class ForgotPasswordConfirmationModel : PageModel
{
    public string? Email { get; private set; }

    public bool Delivered { get; private set; } = true;

    /// <summary>Development only, when SMTP is not configured: the link that would have been mailed.</summary>
    public string? DevResetUrl { get; private set; }

    public void OnGet(string? email = null)
    {
        Email = email;
        Delivered = TempData["ResetEmailDelivered"] as bool? ?? true;
        DevResetUrl = TempData["DevResetUrl"] as string;
    }
}
