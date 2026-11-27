using PopPop.Data.Entities.Enums;
using PopPop.Models;

namespace PopPop.Services.Interfaces;

/// <summary>
/// Order flow for cash on delivery and VNPAY checkout.
/// Business-rule violations throw <see cref="InvalidOperationException"/> with a user-facing message;
/// missing orders throw <see cref="KeyNotFoundException"/>.
/// </summary>
public interface IOrderService
{
    ShippingOptions ShippingPolicy { get; }
    decimal CalculateShippingFee(decimal subtotal);

    /// <summary>Creates an order from the customer's cart and returns its id and server-calculated total.</summary>
    Task<(int OrderId, decimal TotalAmount)> PlaceOrderAsync(
        string userId,
        CheckoutViewModel checkout,
        PaymentMethod paymentMethod,
        CancellationToken cancellationToken = default);
    /// <summary>
    /// Applies a signature-checked VNPAY result (IPN or return URL; idempotent). Returns the VNPAY IPN code:
    /// 00 recorded, 01 order not found, 02 already handled, 04 wrong amount.
    /// </summary>
    Task<string> ProcessVnPayNotificationAsync(
        int orderId,
        long amount,
        bool paymentSucceeded,
        string? transactionNo,
        CancellationToken cancellationToken = default);
    /// <summary>Customer gives up online payment for an unpaid VNPAY order and pays cash on delivery instead.</summary>
    Task SwitchToCashOnDeliveryAsync(int orderId, string userId, CancellationToken cancellationToken = default);
    /// <summary>Cancels unpaid VNPAY orders whose payment window has passed and returns their stock. Returns how many.</summary>
    Task<int> CancelExpiredOnlinePaymentsAsync(CancellationToken cancellationToken = default);
    Task<CustomerOrdersViewModel> GetOrdersForUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<OrderDetailsViewModel?> GetDetailsForUserAsync(int orderId, string userId, CancellationToken cancellationToken = default);
    /// <summary>Fills recipient fields from the customer's most recent order, if any. Returns false when there is none.</summary>
    Task<bool> PrefillCheckoutFromLastOrderAsync(string userId, CheckoutViewModel checkout, CancellationToken cancellationToken = default);
    /// <summary>Customer cancels their own order while it is still waiting for confirmation.</summary>
    Task CancelByCustomerAsync(int orderId, string userId, string? reason, CancellationToken cancellationToken = default);

    Task<AdminDashboardViewModel> GetAdminDashboardAsync(CancellationToken cancellationToken = default);
    Task<OrderManagementListViewModel> GetManagementOrdersAsync(OrderStatus? status, string? search, CancellationToken cancellationToken = default);
    Task<OrderManagementDetailsViewModel?> GetManagementOrderDetailsAsync(int orderId, CancellationToken cancellationToken = default);
    Task UpdateOrderStatusAsync(int orderId, OrderStatus nextStatus, string? cancelReason, string actorName, CancellationToken cancellationToken = default);
    /// <summary>Moves each order one step forward (Pending → Confirmed → … → Completed); never cancels.</summary>
    Task<BulkOrderUpdateResult> AdvanceOrdersAsync(IReadOnlyCollection<int> orderIds, string actorName, CancellationToken cancellationToken = default);
}
