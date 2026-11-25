using PopPop.Data.Entities;

namespace PopPop.Services.Interfaces;

/// <summary>
/// Passwords without email: an administrator sets a temporary password and the user must replace it at the next sign-in.
/// </summary>
public interface IAccountPasswordService
{
    /// <summary>
    /// Replaces the user's password with a generated temporary one, signs out their other sessions and flags the
    /// account so the user must choose a new password. Returns the temporary password (shown once to the admin).
    /// </summary>
    Task<string> ResetToTemporaryPasswordAsync(User user);

    /// <summary>A random temporary password that satisfies the password policy and is easy to read over the phone.</summary>
    string GenerateTemporaryPassword();
}
