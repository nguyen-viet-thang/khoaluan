using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using PopPop.Data.Entities;

namespace PopPop.Services.Implementations;

/// <summary>Adds app-specific claims to the sign-in cookie so middleware does not need a database query per request.</summary>
public sealed class PopPopClaimsPrincipalFactory : UserClaimsPrincipalFactory<User, Role>
{
    /// <summary>Present while the user still has to replace an admin-set password.</summary>
    public const string MustChangePasswordClaim = "poppop:must_change_password";

    public PopPopClaimsPrincipalFactory(UserManager<User> userManager, RoleManager<Role> roleManager, IOptions<IdentityOptions> options)
        : base(userManager, roleManager, options)
    {
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(User user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        if (user.MustChangePassword)
        {
            identity.AddClaim(new Claim(MustChangePasswordClaim, "true"));
        }

        return identity;
    }
}
