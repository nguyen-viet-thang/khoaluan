using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PopPop.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class ResetPasswordConfirmationModel : PageModel
{
    /// <summary>Staff/admin accounts sign in on the management login page instead.</summary>
    public bool Management { get; private set; }

    public void OnGet(bool management = false) => Management = management;
}
