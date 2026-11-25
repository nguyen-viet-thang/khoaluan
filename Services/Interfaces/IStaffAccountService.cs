using PopPop.Models;

namespace PopPop.Services.Interfaces;

/// <summary>
/// Staff account management. Staff sign in with their phone number; the admin sets the first password
/// (or a temporary one when it is forgotten) and the staff member must replace it at the next sign-in.
/// Business-rule violations throw <see cref="InvalidOperationException"/>; unknown staff throw <see cref="KeyNotFoundException"/>.
/// </summary>
public interface IStaffAccountService
{
    Task<StaffAccountListViewModel> GetListAsync(string? search, string? status, CancellationToken cancellationToken = default);
    Task<EditStaffAccountViewModel?> GetEditAsync(string staffId, CancellationToken cancellationToken = default);

    /// <summary>Creates the account; returns the initial password (typed by the admin or generated) to hand over.</summary>
    Task<string> CreateAsync(CreateStaffAccountViewModel model, CancellationToken cancellationToken = default);

    /// <summary>Updates name and phone number; changing the phone number also changes the login.</summary>
    Task UpdateAsync(EditStaffAccountViewModel model, CancellationToken cancellationToken = default);

    /// <summary>Forgotten password: returns a temporary password the staff member must change at the next sign-in.</summary>
    Task<string> ResetPasswordAsync(string staffId, CancellationToken cancellationToken = default);

    Task LockAsync(string staffId, string reason, CancellationToken cancellationToken = default);
    Task UnlockAsync(string staffId, CancellationToken cancellationToken = default);
}
