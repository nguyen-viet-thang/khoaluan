using PopPop.Models;

namespace PopPop.Services.Interfaces;

/// <summary>
/// Back-office customer management. Business-rule violations throw <see cref="InvalidOperationException"/>
/// with a user-facing message; unknown customers throw <see cref="KeyNotFoundException"/>.
/// </summary>
public interface ICustomerManagementService
{
    Task<CustomerListViewModel> GetListAsync(string? search, string? status, string? sort, int page, CancellationToken cancellationToken = default);
    Task<CustomerDetailsViewModel?> GetDetailsAsync(string customerId, CancellationToken cancellationToken = default);
    /// <summary>Locks the account indefinitely; the customer can no longer sign in and active sessions end shortly.</summary>
    Task LockAsync(string customerId, string reason, CancellationToken cancellationToken = default);
    Task UnlockAsync(string customerId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Permanently deletes a customer account that has no orders and no warranty claims
    /// (<paramref name="confirmPhone"/> must repeat the login phone number). Returns the deleted phone number.
    /// </summary>
    Task<string> DeleteAsync(string customerId, string confirmPhone, CancellationToken cancellationToken = default);

    /// <summary>
    /// Forgotten password (no email in this shop): sets a temporary password the admin gives to the customer;
    /// the customer must change it at the next sign-in. Returns the temporary password.
    /// </summary>
    Task<string> ResetPasswordAsync(string customerId, CancellationToken cancellationToken = default);
}
