using Microsoft.AspNetCore.Identity;
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
    private readonly IAccountPasswordService _accountPasswordService;

    public StaffAccountService(
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        IAccountLockService accountLockService,
        IAccountPasswordService accountPasswordService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _accountLockService = accountLockService;
        _accountPasswordService = accountPasswordService;
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

        var phone = RequireValidPhone(model.PhoneNumber);
        if (await _userManager.FindByNameAsync(phone) is not null)
        {
            throw new InvalidOperationException($"Số điện thoại {phone} đã thuộc một tài khoản khác (khách hàng hoặc nhân viên).");
        }

        var password = string.IsNullOrWhiteSpace(model.InitialPassword)
            ? _accountPasswordService.GenerateTemporaryPassword()
            : model.InitialPassword;
        var user = new User
        {
            UserName = phone,
            PhoneNumber = phone,
            FullName = model.FullName.Trim(),
            CreatedAt = DateTime.UtcNow,
            // The admin knows this password: the staff member must replace it at the first sign-in.
            MustChangePassword = true
        };

        EnsureSucceeded(await _userManager.CreateAsync(user, password));
        var roleResult = await _userManager.AddToRoleAsync(user, StaffRole);
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            EnsureSucceeded(roleResult);
        }

        return password;
    }

    public async Task UpdateAsync(EditStaffAccountViewModel model, CancellationToken cancellationToken = default)
    {
        var user = await FindStaffAsync(model.Id);
        var phone = RequireValidPhone(model.PhoneNumber);
        if (!string.Equals(phone, user.UserName, StringComparison.Ordinal))
        {
            // The phone number is the login: renaming checks it is not used by another account.
            EnsureSucceeded(await _userManager.SetUserNameAsync(user, phone));
        }

        user.FullName = model.FullName.Trim();
        user.PhoneNumber = phone;
        EnsureSucceeded(await _userManager.UpdateAsync(user));
    }

    public async Task<string> ResetPasswordAsync(string staffId, CancellationToken cancellationToken = default)
    {
        var user = await FindStaffAsync(staffId);
        if (_accountLockService.IsLocked(user))
        {
            throw new InvalidOperationException("Tài khoản đang bị khóa. Hãy mở khóa trước khi cấp mật khẩu tạm.");
        }

        return await _accountPasswordService.ResetToTemporaryPasswordAsync(user);
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
