using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using PopPop.Data.Entities;
using PopPop.Services.Interfaces;

namespace PopPop.Services.Implementations;

public sealed class AccountPasswordService : IAccountPasswordService
{
    private readonly UserManager<User> _userManager;

    public AccountPasswordService(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<string> ResetToTemporaryPasswordAsync(User user)
    {
        var temporary = GenerateTemporaryPassword();
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, temporary);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException("Không thể đặt lại mật khẩu: " + string.Join(" ", result.Errors.Select(error => error.Description)));
        }

        // ResetPasswordAsync already rotates the security stamp, so existing sessions end within a minute.
        user.MustChangePassword = true;
        user.AccessFailedCount = 0;
        var update = await _userManager.UpdateAsync(user);
        if (!update.Succeeded)
        {
            throw new InvalidOperationException("Không thể cập nhật tài khoản: " + string.Join(" ", update.Errors.Select(error => error.Description)));
        }

        return temporary;
    }

    /// <summary>"Pp@" + 6 random digits: upper, lower, symbol and digits for the default Identity policy.</summary>
    public string GenerateTemporaryPassword() => $"Pp@{RandomNumberGenerator.GetInt32(0, 1_000_000):000000}";
}
