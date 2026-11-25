using Microsoft.AspNetCore.Identity;
using PopPop.Data.Entities;

namespace PopPop.Services.Implementations;

/// <summary>
/// An account that signs in with an email must have verified it; phone-number accounts (staff, admin,
/// older customers) have no email and are always allowed. Used by SignInManager because
/// SignIn.RequireConfirmedAccount is on: an unverified account gets SignInResult.NotAllowed.
/// </summary>
public sealed class EmailUserConfirmation : IUserConfirmation<User>
{
    public Task<bool> IsConfirmedAsync(UserManager<User> manager, User user) =>
        Task.FromResult(string.IsNullOrEmpty(user.Email) || user.EmailConfirmed);
}
