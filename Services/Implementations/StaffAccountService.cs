using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PopPop.Data.Entities;
using PopPop.Models;
using PopPop.Services.Interfaces;

namespace PopPop.Services.Implementations;

public sealed class StaffAccountService : IStaffAccountService
{
    private const string StaffRole = "Staff";
    private const string AdminRole = "Admin";

    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly IAccountLockService _accountLockService;

    public StaffAccountService(
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        IAccountLockService accountLockService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _accountLockService = accountLockService;
    }

    public async Task<StaffAccountListViewModel> GetListAsync(
        string? search,
        string? status,
        CancellationToken cancellationToken = default)
    {
        var normalizedStatus = status is "active" or "pending" or "locked" ? status : null;
        var normalizedSearch = string.IsNullOrWhiteSpace(search) ? null : search.Trim();

        var users = await _userManager.GetUsersInRoleAsync(StaffRole);
        var accounts = new List<StaffAccountItemViewModel>(users.Count);
        foreach (var user in users)
        {
            // An admin who also holds the Staff role is managed elsewhere.
            if (await _userManager.IsInRoleAsync(user, AdminRole))
            {
                continue;
            }

            accounts.Add(ToItem(user));
        }

        IEnumerable<StaffAccountItemViewModel> filtered = accounts;
        if (normalizedSearch is not null)
        {
            var phoneTerm = PhoneNumberFormatter.Normalize(normalizedSearch) ?? normalizedSearch;
            filtered = filtered.Where(account =>
                account.FullName.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase)
                || (account.Email?.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ?? false)
                || account.PhoneNumber.Contains(phoneTerm, StringComparison.Ordinal));
        }

        filtered = normalizedStatus switch
        {
            "locked" => filtered.Where(account => account.IsLocked),
            "pending" => filtered.Where(account => !account.IsLocked && account.MustChangePassword),
            "active" => filtered.Where(account => !account.IsLocked && !account.MustChangePassword),
            _ => filtered
        };

        return new StaffAccountListViewModel
        {
            Accounts = filtered.OrderBy(account => account.FullName).ToList(),
            Search = normalizedSearch,
            Status = normalizedStatus
        };
    }

    public async Task<EditStaffAccountViewModel?> GetEditAsync(string staffId, CancellationToken cancellationToken = default)
    {
        var user = await FindStaffOrDefaultAsync(staffId);
        if (user is null)
        {
            return null;
        }

        return new EditStaffAccountViewModel
        {
            Id = user.Id,
            FullName = user.FullName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            PhoneNumber = LoginOf(user),
            MustChangePassword = user.MustChangePassword,
            IsLocked = _accountLockService.IsLocked(user),
            LockReason = user.LockReason,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<string> CreateAsync(CreateStaffAccountViewModel model, CancellationToken cancellationToken = default)
    {
        if (!await _roleManager.RoleExistsAsync(StaffRole))
        {
            throw new InvalidOperationException("Không tìm thấy vai trò Staff. Vui lòng kiểm tra dữ liệu phân quyền.");
        }

        var email = RequireGmail(model.Email);
        var phone = RequireValidPhone(model.PhoneNumber);
        await EnsureEmailAvailableAsync(email, exceptUserId: null);
        await EnsurePhoneAvailableAsync(phone, exceptUserId: null);

        var user = new User
        {
            // The staff member's own Gmail is the login; the phone number is the initial password.
            UserName = email,
            Email = email,
            // Entered by the admin for an employee: no verification mail is needed to sign in.
            EmailConfirmed = true,
            PhoneNumber = phone,
            FullName = model.FullName.Trim(),
            CreatedAt = DateTime.UtcNow,
            // The admin knows this password: the staff member must replace it at the first sign-in.
            MustChangePassword = true
        };
        // Hashed directly: a phone number (digits only) would fail the password policy, which still
        // applies to the new password the staff member must choose at the first sign-in.
        user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, phone);

        EnsureSucceeded(await _userManager.CreateAsync(user));
        var roleResult = await _userManager.AddToRoleAsync(user, StaffRole);
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            EnsureSucceeded(roleResult);
        }

        return phone;
    }

    public async Task UpdateAsync(EditStaffAccountViewModel model, CancellationToken cancellationToken = default)
    {
        var user = await FindStaffAsync(model.Id);
        var email = RequireGmail(model.Email);
        var phone = RequireValidPhone(model.PhoneNumber);
        await EnsureEmailAvailableAsync(email, user.Id);
        await EnsurePhoneAvailableAsync(phone, user.Id);

        if (!string.Equals(email, user.UserName, StringComparison.Ordinal))
        {
            // The Gmail is the login (older staff accounts switch from their phone number to it here).
            EnsureSucceeded(await _userManager.SetUserNameAsync(user, email));
        }

        user.Email = email;
        user.NormalizedEmail = _userManager.NormalizeEmail(email);
        user.EmailConfirmed = true;
        user.FullName = model.FullName.Trim();
        user.PhoneNumber = phone;
        EnsureSucceeded(await _userManager.UpdateAsync(user));
    }

    public async Task<string> ResetPasswordAsync(string staffId, CancellationToken cancellationToken = default)
    {
        var user = await FindStaffAsync(staffId);
        if (_accountLockService.IsLocked(user))
        {
            throw new InvalidOperationException("Tài khoản đang bị khóa. Hãy mở khóa trước khi đặt lại mật khẩu.");
        }

        var phone = RequireValidPhone(user.PhoneNumber ?? user.UserName);
        user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, phone);
        user.MustChangePassword = true;
        user.AccessFailedCount = 0;
        // Rotating the stamp also saves the user and signs the old sessions out within a minute.
        EnsureSucceeded(await _userManager.UpdateSecurityStampAsync(user));
        return phone;
    }

    public async Task LockAsync(string staffId, string reason, CancellationToken cancellationToken = default)
    {
        var user = await FindStaffAsync(staffId);
        await _accountLockService.LockAsync(user, reason);
    }

    public async Task UnlockAsync(string staffId, CancellationToken cancellationToken = default)
    {
        var user = await FindStaffAsync(staffId);
        await _accountLockService.UnlockAsync(user);
    }

    private static string RequireValidPhone(string? input)
    {
        var phone = PhoneNumberFormatter.Normalize(input);
        return PhoneNumberFormatter.IsValid(phone) ? phone! : throw new InvalidOperationException(PhoneNumberFormatter.InvalidMessage);
    }

    private static string RequireGmail(string? input)
    {
        var email = (input ?? string.Empty).Trim().ToLowerInvariant();
        return System.Text.RegularExpressions.Regex.IsMatch(email, StaffAccountRules.GmailPattern)
            ? email
            : throw new InvalidOperationException(StaffAccountRules.GmailMessage);
    }

    private async Task EnsureEmailAvailableAsync(string email, string? exceptUserId)
    {
        var owner = await _userManager.FindByNameAsync(email) ?? await _userManager.FindByEmailAsync(email);
        if (owner is not null && owner.Id != exceptUserId)
        {
            throw new InvalidOperationException($"Gmail {email} đã thuộc một tài khoản khác (khách hàng hoặc nhân viên).");
        }
    }

    /// <summary>
    /// Staff can sign in to the management area with their phone number too, so it must not be another
    /// account's phone login nor the phone of another staff member or admin.
    /// </summary>
    private async Task EnsurePhoneAvailableAsync(string phone, string? exceptUserId)
    {
        var message = $"Số điện thoại {phone} đã thuộc một tài khoản khác (khách hàng hoặc nhân viên).";
        var phoneLogin = await _userManager.FindByNameAsync(phone);
        if (phoneLogin is not null && phoneLogin.Id != exceptUserId)
        {
            throw new InvalidOperationException(message);
        }

        var samePhone = await _userManager.Users
            .Where(user => user.PhoneNumber == phone && user.Id != exceptUserId)
            .ToListAsync();
        foreach (var other in samePhone)
        {
            if (await _userManager.IsInRoleAsync(other, StaffRole) || await _userManager.IsInRoleAsync(other, AdminRole))
            {
                throw new InvalidOperationException(message);
            }
        }
    }

    private static string LoginOf(User user) => user.PhoneNumber ?? user.UserName ?? string.Empty;

    private async Task<User?> FindStaffOrDefaultAsync(string staffId)
    {
        var user = await _userManager.FindByIdAsync(staffId);
        if (user is null
            || !await _userManager.IsInRoleAsync(user, StaffRole)
            || await _userManager.IsInRoleAsync(user, AdminRole))
        {
            return null;
        }

        return user;
    }

    private async Task<User> FindStaffAsync(string staffId) =>
        await FindStaffOrDefaultAsync(staffId) ?? throw new KeyNotFoundException("Không tìm thấy nhân viên.");

    private StaffAccountItemViewModel ToItem(User user) => new()
    {
        Id = user.Id,
        FullName = user.FullName ?? string.Empty,
        Email = string.IsNullOrEmpty(user.Email) ? null : user.Email,
        PhoneNumber = LoginOf(user),
        MustChangePassword = user.MustChangePassword,
        IsLocked = _accountLockService.IsLocked(user),
        LockReason = user.LockReason,
        CreatedAt = user.CreatedAt
    };

    private static void EnsureSucceeded(IdentityResult result)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join(" ", result.Errors.Select(error => error.Description)));
        }
    }
}
