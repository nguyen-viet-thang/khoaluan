using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using PopPop.Data.Entities;
using PopPop.Services.Interfaces;

namespace PopPop.Areas.Identity.Pages.Account;

/// <summary>Builds the "verify your email" link (/Identity/Account/ConfirmEmail?userId=…&amp;code=…) and mails it.</summary>
internal static class EmailConfirmationLink
{
    public const string EmailAddressPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

    /// <summary>Lower-cased, trimmed email used as both Email and UserName of customer accounts.</summary>
    public static string Normalize(string email) => email.Trim().ToLowerInvariant();

    public static async Task<(bool Delivered, string Url)> SendAsync(
        PageModel page, UserManager<User> userManager, IAccountEmailService accountEmailService, User user)
    {
        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var url = page.Url.Page(
            "/Account/ConfirmEmail",
            pageHandler: null,
            values: new { area = "Identity", userId = user.Id, code },
            protocol: page.Request.Scheme)!;

        var delivered = await accountEmailService.SendEmailConfirmationAsync(user.Email!, user.FullName, url, page.HttpContext.RequestAborted);
        return (delivered, url);
    }
}
