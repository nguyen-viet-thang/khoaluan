using PopPop.Data.Entities;
using PopPop.Models;

namespace PopPop.Repositories.Interfaces;

/// <summary>
/// Customers are all accounts that are not Admin or Staff.
/// </summary>
public interface ICustomerRepository : IRepository<User, string>
{
    Task<(IReadOnlyList<CustomerSummary> Customers, int TotalCount)> GetPageAsync(
        string? search,
        bool? isLocked,
        string sort,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<CustomerSummary?> GetSummaryAsync(string customerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetOrdersAsync(string customerId, CancellationToken cancellationToken = default);
    Task<bool> IsCustomerAsync(string userId, CancellationToken cancellationToken = default);
    Task<int> CountClaimsAsync(string customerId, CancellationToken cancellationToken = default);
}
