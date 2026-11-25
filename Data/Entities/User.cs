using Microsoft.AspNetCore.Identity;

namespace PopPop.Data.Entities;

/// <summary>
/// Custom User extending IdentityUser
/// </summary>
public class User : IdentityUser, IBaseEntity<string>
{
    /// <summary>
    /// Full name of the user
    /// </summary>
    public string? FullName { get; set; }

    /// <summary>
    /// Avatar path of the user
    /// </summary>
    public string? AvatarPath { get; set; }

    /// <summary>
    /// Registration time (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Why an administrator locked this account (shown in the back office); null when not locked.
    /// </summary>
    public string? LockReason { get; set; }

    /// <summary>
    /// The current password was set by an administrator (new staff account, password reset):
    /// the user must choose their own password before using the site.
    /// </summary>
    public bool MustChangePassword { get; set; }
}
