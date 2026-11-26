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
    Task<string> ProcessVnPayNotificationAsync(
        int orderId,
        long amount,
        bool paymentSucceeded,
        CancellationToken cancellationToken = default);
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
}
