using PopPop.Models;

namespace PopPop.Services.Interfaces;

/// <summary>
/// Staff account management. Staff sign in with their own Gmail (or phone number); the initial password is
/// their phone number and they must replace it at the first sign-in. A forgotten password is reset to the
/// phone number again. Business-rule violations throw <see cref="InvalidOperationException"/>; unknown
/// staff throw <see cref="KeyNotFoundException"/>.
/// </summary>
public interface IStaffAccountService
{
    Task<StaffAccountListViewModel> GetListAsync(string? search, string? status, CancellationToken cancellationToken = default);
    Task<EditStaffAccountViewModel?> GetEditAsync(string staffId, CancellationToken cancellationToken = default);

    /// <summary>Creates the account; returns the initial password (= the normalised phone number) to hand over.</summary>
    Task<string> CreateAsync(CreateStaffAccountViewModel model, CancellationToken cancellationToken = default);

    /// <summary>Updates name, Gmail (= login) and phone number.</summary>
    Task UpdateAsync(EditStaffAccountViewModel model, CancellationToken cancellationToken = default);

    /// <summary>Forgotten password: resets it to the phone number; the staff member must change it at the next sign-in.</summary>
    Task<string> ResetPasswordAsync(string staffId, CancellationToken cancellationToken = default);

    Task LockAsync(string staffId, string reason, CancellationToken cancellationToken = default);
    Task UnlockAsync(string staffId, CancellationToken cancellationToken = default);
}
