using Microsoft.AspNetCore.Identity;
using PopPop.Data.Entities;
using PopPop.Services.Interfaces;

namespace PopPop.Services.Implementations;

public sealed class AccountLockService : IAccountLockService
{
    private const int MaxReasonLength = 500;
    private readonly UserManager<User> _userManager;

    public AccountLockService(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task LockAsync(User user, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new InvalidOperationException("Vui lòng nhập lý do khóa tài khoản.");
        }

        var trimmed = reason.Trim();
        user.LockReason = trimmed.Length > MaxReasonLength ? trimmed[..MaxReasonLength] : trimmed;
        await EnsureSucceededAsync(_userManager.SetLockoutEnabledAsync(user, true));
        await EnsureSucceededAsync(_userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue));
        // Changing the security stamp makes existing sign-in cookies invalid at the next validation.
        await EnsureSucceededAsync(_userManager.UpdateSecurityStampAsync(user));
    }

    public async Task UnlockAsync(User user)
    {
        user.LockReason = null;
        await EnsureSucceededAsync(_userManager.SetLockoutEndDateAsync(user, null));
        await EnsureSucceededAsync(_userManager.ResetAccessFailedCountAsync(user));
    }

    public bool IsLocked(User user) => user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow;

    private static async Task EnsureSucceededAsync(Task<IdentityResult> operation)
    {
        var result = await operation;
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join(" ", result.Errors.Select(error => error.Description)));
        }
    }
}
