using PopPop.Data.Entities.Enums;

namespace PopPop.Models;

public sealed class OrderManagementRowViewModel
{
    public int Id { get; init; }
    public string OrderCode { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;
    public string CustomerPhone { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public OrderStatus Status { get; init; }
    public string StatusText { get; init; } = string.Empty;
    public PaymentMethod PaymentMethod { get; init; }
    public PaymentStatus PaymentStatus { get; init; }
    public DateTime? PaidAt { get; init; }
    public decimal TotalAmount { get; init; }

    /// <summary>The forward step staff can take with one click (null when the order is finished).</summary>
    public OrderStatus? NextStatus { get; init; }

    /// <summary>VNPAY order the customer has not paid yet: the shop must wait before processing it.</summary>
    public bool AwaitingOnlinePayment { get; init; }
}

public sealed class AdminDashboardViewModel
{
    public int TotalOrders { get; init; }
    public int PendingOrders { get; init; }
    public int ConfirmedOrders { get; init; }
    public int ProcessingOrders { get; init; }
    public int ShippingOrders { get; init; }
    public int CompletedOrders { get; init; }
    public int CancelledOrders { get; init; }
    public decimal MonthlyRevenue { get; init; }
    public IReadOnlyList<string> DailyChartLabels { get; init; } = [];
    public IReadOnlyList<int> DailyOrderCounts { get; init; } = [];
    public IReadOnlyList<decimal> DailyCompletedRevenue { get; init; } = [];
    public IReadOnlyList<int> StatusChartValues { get; init; } = [];
    public IReadOnlyList<OrderManagementRowViewModel> RecentOrders { get; init; } = [];
}

public sealed class OrderManagementListViewModel
{
    public OrderStatus? Status { get; set; }
    public string? Search { get; set; }
    public IReadOnlyList<OrderManagementRowViewModel> Orders { get; set; } = [];

    /// <summary>Orders per status for the status tabs (respects the search, ignores the status filter).</summary>
    public IReadOnlyDictionary<OrderStatus, int> StatusCounts { get; set; } = new Dictionary<OrderStatus, int>();
    public int TotalCount => StatusCounts.Values.Sum();
    public int OpenCount => StatusCounts.Where(pair => pair.Key is not (OrderStatus.Completed or OrderStatus.Cancelled)).Sum(pair => pair.Value);
}

/// <summary>Outcome of moving several orders one step forward at once.</summary>
public sealed class BulkOrderUpdateResult
{
    public List<string> Updated { get; } = [];
    public List<string> Failed { get; } = [];
}

public sealed class OrderManagementDetailsViewModel
{
    public int Id { get; init; }
    public string OrderCode { get; init; } = string.Empty;
    public OrderStatus Status { get; init; }
    public string StatusText { get; init; } = string.Empty;
    public PaymentMethod PaymentMethod { get; init; }
    public PaymentStatus PaymentStatus { get; init; }
    public DateTime? PaidAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    /// <summary>Phone number the customer account signs in with.</summary>
    public string CustomerAccountPhone { get; init; } = string.Empty;
    public string RecipientName { get; init; } = string.Empty;
    public string RecipientPhone { get; init; } = string.Empty;
    public string ShippingAddress { get; init; } = string.Empty;
    public string? Note { get; init; }
    public decimal ShippingFee { get; init; }
    public decimal TotalAmount { get; init; }
    public IReadOnlyList<OrderLineViewModel> Items { get; init; } = [];
    public IReadOnlyList<OrderStatus> AvailableStatuses { get; init; } = [];

    /// <summary>VNPAY order still unpaid: deadline (UTC) after which it is cancelled automatically.</summary>
    public DateTime? PaymentDeadline { get; init; }
    public bool AwaitingOnlinePayment => PaymentDeadline.HasValue;
}
