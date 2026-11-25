using PopPop.Data.Entities.Enums;

namespace PopPop.Models;

/// <summary>One customer row with order aggregates (projected by the repository).</summary>
public sealed record CustomerSummary(
    string Id,
    string? FullName,
    /// <summary>Phone number (for phone accounts it is also the login).</summary>
    string? PhoneNumber,
    /// <summary>Login email (null for accounts registered with a phone number).</summary>
    string? Email,
    bool EmailConfirmed,
    DateTime CreatedAt,
    bool IsLocked,
    int OrderCount,
    decimal TotalSpent,
    DateTime? LastOrderAt)
{
    /// <summary>What the customer signs in with: the email, or the phone number for older accounts.</summary>
    public string? Login => Email ?? PhoneNumber;
}

public sealed class CustomerListViewModel
{
    public IReadOnlyList<CustomerSummary> Customers { get; init; } = [];
    public string? Search { get; init; }
    public string? Status { get; init; }
    public string Sort { get; init; } = "newest";
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}

public sealed class CustomerDetailsViewModel
{
    public required CustomerSummary Customer { get; init; }
    public string? LockReason { get; init; }
    public DateTimeOffset? LockoutEnd { get; init; }
    /// <summary>Delivery details of the most recent order (the customer's usual address).</summary>
    public string? LastShippingAddress { get; init; }
    public string? LastRecipientPhone { get; init; }
    public int CompletedOrders { get; init; }
    public int CancelledOrders { get; init; }
    public int OpenOrders { get; init; }
    public decimal AverageOrderValue { get; init; }
    public IReadOnlyList<CustomerOrderRow> Orders { get; init; } = [];
    public int ClaimCount { get; init; }

    /// <summary>
    /// Only accounts without orders or warranty claims can be deleted: those are sales/audit records
    /// that must keep their owner. Accounts with history can be locked instead.
    /// </summary>
    public bool CanDelete => Orders.Count == 0 && ClaimCount == 0;
}

public sealed class CustomerOrderRow
{
    public int Id { get; init; }
    public required string OrderCode { get; init; }
    public DateTime CreatedAt { get; init; }
    public OrderStatus Status { get; init; }
    public required string StatusText { get; init; }
    public int ItemCount { get; init; }
    public decimal TotalAmount { get; init; }
}
