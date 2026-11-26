using Microsoft.Extensions.Options;
using PopPop.Data.Entities;
using PopPop.Data.Entities.Enums;
using PopPop.Models;
using PopPop.Repositories.Interfaces;
using PopPop.Services.Interfaces;

namespace PopPop.Services.Implementations;

public sealed class OrderService : IOrderService
{
    private readonly ICartRepository _cartRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IRepository<InventoryTransaction, int> _inventoryTransactionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ShippingOptions _shipping;
    private readonly IReportRepository _reportRepository;
    private readonly INotificationService _notificationService;

    public OrderService(
        ICartRepository cartRepository,
        IOrderRepository orderRepository,
        IRepository<InventoryTransaction, int> inventoryTransactionRepository,
        IUnitOfWork unitOfWork,
        IOptions<ShippingOptions> shippingOptions,
        IReportRepository reportRepository,
        INotificationService notificationService)
    {
        _cartRepository = cartRepository;
        _orderRepository = orderRepository;
        _reportRepository = reportRepository;
        _notificationService = notificationService;
        _inventoryTransactionRepository = inventoryTransactionRepository;
        _unitOfWork = unitOfWork;
        _shipping = shippingOptions.Value;
    }

    #region Customer

    public ShippingOptions ShippingPolicy => _shipping;

    public decimal CalculateShippingFee(decimal subtotal) => _shipping.CalculateFee(subtotal);

    public async Task<(int OrderId, decimal TotalAmount)> PlaceOrderAsync(
        string userId,
        CheckoutViewModel checkout,
        PaymentMethod paymentMethod,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new InvalidOperationException("Bạn cần đăng nhập để đặt hàng.");
        }

        if (paymentMethod is not (PaymentMethod.CashOnDelivery or PaymentMethod.VnPay))
        {
            throw new InvalidOperationException("Phương thức thanh toán không hợp lệ.");
        }

        var order = await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var cart = await _cartRepository.GetByUserIdAsync(userId, token);
            if (cart is null || cart.CartItems.Count == 0)
            {
                throw new InvalidOperationException("Giỏ hàng của bạn đang trống.");
            }

            foreach (var cartItem in cart.CartItems)
            {
                var variant = cartItem.ProductVariant;
                if (variant is null || !variant.IsActive || variant.Product?.IsOnSale() != true)
                {
                    throw new InvalidOperationException("Một sản phẩm trong giỏ hiện không còn khả dụng.");
                }

                if (cartItem.Quantity <= 0 || cartItem.Quantity > variant.StockQuantity)
                {
                    throw new InvalidOperationException($"Sản phẩm {variant.Product.Name} không đủ số lượng tồn kho.");
                }
            }

            var now = DateTime.UtcNow;
            var orderCode = await ShortCodeGenerator.CreateUniqueAsync("PP", now, _orderRepository.OrderCodeExistsAsync, token);
            var newOrder = new Order
            {
                UserId = userId,
                OrderCode = orderCode,
                Status = OrderStatus.Pending,
                PaymentMethod = paymentMethod,
                PaymentStatus = PaymentStatus.Unpaid,
                RecipientName = checkout.RecipientName.Trim(),
                RecipientPhone = PhoneNumberFormatter.Normalize(checkout.RecipientPhone),
                ShippingAddress = checkout.ShippingAddress.Trim(),
                Note = string.IsNullOrWhiteSpace(checkout.Note) ? null : checkout.Note.Trim(),
                CreatedAt = now,
                UpdatedAt = now
            };

            foreach (var cartItem in cart.CartItems)
            {
                var variant = cartItem.ProductVariant!;
                var product = variant.Product!;

                newOrder.OrderItems.Add(new OrderItem
                {
                    ProductVariantId = variant.Id,
                    ProductName = product.Name,
                    SKU = variant.SKU,
                    UnitPrice = variant.Price,
                    Quantity = cartItem.Quantity,
                    TotalPrice = variant.Price * cartItem.Quantity
                });

                variant.StockQuantity -= cartItem.Quantity;
                await _inventoryTransactionRepository.AddAsync(new InventoryTransaction
                {
                    ProductVariantId = variant.Id,
                    Type = InventoryTransactionType.Sale,
                    Quantity = -cartItem.Quantity,
                    Note = $"Xuất kho cho đơn hàng {orderCode}",
                    CreatedAt = now
                }, token);
            }

            var subtotal = newOrder.OrderItems.Sum(item => item.TotalPrice);
            newOrder.ShippingFee = _shipping.CalculateFee(subtotal);
            newOrder.TotalAmount = subtotal + newOrder.ShippingFee;
            cart.CartItems.Clear();
            cart.UpdatedAt = now;
            await _orderRepository.AddAsync(newOrder, token);
            return newOrder;
        }, cancellationToken);

        // After commit: tell the back office there is a new order to confirm.
        await _notificationService.NotifyStaffAsync(
            $"Đơn hàng mới {order.OrderCode}",
            $"{order.RecipientName} · {order.OrderItems.Sum(item => item.Quantity)} sản phẩm · {order.TotalAmount:N0} ₫ ({(order.PaymentMethod == PaymentMethod.VnPay ? "VNPAY" : "COD")}). Đang chờ xác nhận.",
            $"/Staff/Orders/Details/{order.Id}",
            cancellationToken);
        return (order.Id, order.TotalAmount);
    }

    public Task<string> ProcessVnPayNotificationAsync(
        int orderId,
        long amount,
        bool paymentSucceeded,
        CancellationToken cancellationToken = default)
    {
        return _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var order = await _orderRepository.GetByIdAsync(orderId, token);
            if (order is null || order.PaymentMethod != PaymentMethod.VnPay)
            {
                return "01";
            }

            if (amount != checked((long)(order.TotalAmount * 100m)))
            {
                return "04";
            }

            if (order.PaymentStatus == PaymentStatus.Paid || order.Status == OrderStatus.Cancelled)
            {
                return "02";
            }

            if (paymentSucceeded)
            {
                var now = DateTime.UtcNow;
                order.PaymentStatus = PaymentStatus.Paid;
                order.PaidAt = now;
                order.UpdatedAt = now;
            }

            return "00";
        }, cancellationToken);
    }

    public async Task<OrderDetailsViewModel?> GetDetailsForUserAsync(
        int orderId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetDetailsForUserAsync(orderId, userId, cancellationToken);
        if (order is null)
        {
            return null;
        }

        return new OrderDetailsViewModel
        {
            Id = order.Id,
            OrderCode = order.OrderCode,
            CanCancel = CanCustomerCancel(order),
            Status = order.Status,
            PaymentMethod = order.PaymentMethod,
            PaymentStatus = order.PaymentStatus,
            PaidAt = order.PaidAt,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            RecipientName = order.RecipientName ?? string.Empty,
            RecipientPhone = order.RecipientPhone ?? string.Empty,
            ShippingAddress = order.ShippingAddress ?? string.Empty,
            Note = order.Note,
            Items = order.OrderItems.Select(ToLine).ToList(),
            ShippingFee = order.ShippingFee,
            TotalAmount = order.TotalAmount
        };
    }

    public async Task<bool> PrefillCheckoutFromLastOrderAsync(
        string userId,
        CheckoutViewModel checkout,
        CancellationToken cancellationToken = default)
    {
        var lastOrder = await _orderRepository.GetLatestForUserAsync(userId, cancellationToken);
        if (lastOrder is null)
        {
            return false;
        }

        checkout.RecipientName = lastOrder.RecipientName ?? string.Empty;
        checkout.RecipientPhone = lastOrder.RecipientPhone ?? string.Empty;
        checkout.ShippingAddress = lastOrder.ShippingAddress ?? string.Empty;
        return true;
    }

    public async Task CancelByCustomerAsync(
        int orderId,
        string userId,
        string? reason,
        CancellationToken cancellationToken = default)
    {
        var cancelled = await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var order = await _orderRepository.GetDetailsForManagementAsync(orderId, token);
            if (order is null || order.UserId != userId)
            {
                throw new KeyNotFoundException("Không tìm thấy đơn hàng.");
            }

            if (!CanCustomerCancel(order))
            {
                throw new InvalidOperationException(order.Status == OrderStatus.Cancelled
                    ? "Đơn hàng đã được hủy trước đó."
                    : "Đơn hàng đã được cửa hàng xác nhận nên không thể tự hủy. Vui lòng liên hệ cửa hàng để được hỗ trợ.");
            }

            var now = DateTime.UtcNow;
            await ReleaseReservedStockAsync(order, $"Hoàn kho do khách hủy đơn hàng {order.OrderCode}", now, token);
            AppendNote(order, string.IsNullOrWhiteSpace(reason)
                ? "Khách hàng đã hủy đơn."
                : $"Khách hàng đã hủy đơn. Lý do: {reason.Trim()}");
            order.Status = OrderStatus.Cancelled;
            order.UpdatedAt = now;
            return order;
        }, cancellationToken);

        await _notificationService.NotifyStaffAsync(
            $"Khách đã hủy đơn {cancelled.OrderCode}",
            (string.IsNullOrWhiteSpace(reason) ? "Khách không ghi lý do." : $"Lý do: {reason.Trim()}.")
                + " Tồn kho đã được hoàn lại; không cần chuẩn bị hàng cho đơn này.",
            $"/Staff/Orders/Details/{cancelled.Id}",
            cancellationToken);
    }

    public async Task<CustomerOrdersViewModel> GetOrdersForUserAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var orders = await _orderRepository.GetForUserAsync(userId, cancellationToken);
        return new CustomerOrdersViewModel
        {
            Orders = orders.Select(order => new CustomerOrderRowViewModel
            {
                Id = order.Id,
                OrderCode = order.OrderCode,
                Status = order.Status,
                PaymentMethod = order.PaymentMethod,
                PaymentStatus = order.PaymentStatus,
                StatusText = GetStatusText(order.Status),
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                TotalAmount = order.TotalAmount
            }).ToList()
        };
    }

    #endregion

    #region Management (Admin & Staff)

    public async Task<AdminDashboardViewModel> GetAdminDashboardAsync(CancellationToken cancellationToken = default)
    {
        var recentOrders = await _orderRepository.GetRecentForManagementAsync(8, cancellationToken);

        // Days and months are Vietnam local time (same rules as the Reports page):
        // orders count on the day they were placed, revenue on the day they were delivered (PaidAt).
        var today = ShopClock.Today;
        var monthStart = new DateOnly(today.Year, today.Month, 1);
        var chartStart = today.AddDays(-6);
        var chartFromUtc = ShopClock.StartOfDayUtc(chartStart);
        var chartToUtc = ShopClock.StartOfDayUtc(today.AddDays(1));
        var chartOrders = await _reportRepository.GetOrdersAsync(chartFromUtc, chartToUtc, cancellationToken);
        var placedByDay = chartOrders
            .Where(order => order.CreatedAt >= chartFromUtc && order.CreatedAt < chartToUtc)
            .GroupBy(order => DateOnly.FromDateTime(ShopClock.ToLocal(order.CreatedAt)))
            .ToDictionary(group => group.Key, group => group.Count());
        var revenueByDay = chartOrders
            .Where(order => order.CompletedAt >= chartFromUtc && order.CompletedAt < chartToUtc)
            .GroupBy(order => DateOnly.FromDateTime(ShopClock.ToLocal(order.CompletedAt!.Value)))
            .ToDictionary(group => group.Key, group => group.Sum(order => order.TotalAmount));
        var chartDates = Enumerable.Range(0, 7).Select(day => chartStart.AddDays(day)).ToList();
        var pendingOrders = await _orderRepository.CountByStatusAsync(OrderStatus.Pending, cancellationToken);
        var confirmedOrders = await _orderRepository.CountByStatusAsync(OrderStatus.Confirmed, cancellationToken);
        var processingOrders = await _orderRepository.CountByStatusAsync(OrderStatus.Processing, cancellationToken);
        var shippingOrders = await _orderRepository.CountByStatusAsync(OrderStatus.Shipping, cancellationToken);
        var completedOrders = await _orderRepository.CountByStatusAsync(OrderStatus.Completed, cancellationToken);
        var cancelledOrders = await _orderRepository.CountByStatusAsync(OrderStatus.Cancelled, cancellationToken);

        return new AdminDashboardViewModel
        {
            TotalOrders = await _orderRepository.CountAllAsync(cancellationToken),
            PendingOrders = pendingOrders,
            ConfirmedOrders = confirmedOrders,
            ProcessingOrders = processingOrders,
            ShippingOrders = shippingOrders,
            CompletedOrders = completedOrders,
            CancelledOrders = cancelledOrders,
            MonthlyRevenue = await _reportRepository.GetCompletedRevenueAsync(
                ShopClock.StartOfDayUtc(monthStart),
                ShopClock.StartOfDayUtc(monthStart.AddMonths(1)),
                cancellationToken),
            DailyChartLabels = chartDates.Select(date => date.ToString("dd/MM", System.Globalization.CultureInfo.InvariantCulture)).ToList(),
            DailyOrderCounts = chartDates.Select(date => placedByDay.GetValueOrDefault(date)).ToList(),
            DailyCompletedRevenue = chartDates.Select(date => revenueByDay.GetValueOrDefault(date)).ToList(),
            StatusChartValues = [pendingOrders, confirmedOrders, processingOrders, shippingOrders, completedOrders, cancelledOrders],
            RecentOrders = recentOrders.Select(ToManagementRow).ToList()
        };
    }

    public async Task<OrderManagementListViewModel> GetManagementOrdersAsync(
        OrderStatus? status,
        string? search,
        CancellationToken cancellationToken = default)
    {
        var orders = await _orderRepository.GetForManagementAsync(status, search, cancellationToken);
        return new OrderManagementListViewModel
        {
            Status = status,
            Search = search,
            Orders = orders.Select(ToManagementRow).ToList()
        };
    }

    public async Task<OrderManagementDetailsViewModel?> GetManagementOrderDetailsAsync(
        int orderId,
        CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetDetailsForManagementAsync(orderId, cancellationToken);
        if (order is null)
        {
            return null;
        }

        return new OrderManagementDetailsViewModel
        {
            Id = order.Id,
            OrderCode = order.OrderCode,
            Status = order.Status,
            StatusText = GetStatusText(order.Status),
            PaymentMethod = order.PaymentMethod,
            PaymentStatus = order.PaymentStatus,
            PaidAt = order.PaidAt,
            CreatedAt = order.CreatedAt,
            CustomerName = order.User?.FullName ?? order.User?.UserName ?? "Khách hàng",
            CustomerAccountPhone = order.User?.PhoneNumber ?? order.User?.UserName ?? string.Empty,
            RecipientName = order.RecipientName ?? string.Empty,
            RecipientPhone = order.RecipientPhone ?? string.Empty,
            ShippingAddress = order.ShippingAddress ?? string.Empty,
            Note = order.Note,
            ShippingFee = order.ShippingFee,
            TotalAmount = order.TotalAmount,
            Items = order.OrderItems.Select(ToLine).ToList(),
            AvailableStatuses = GetAvailableStatuses(order.Status)
        };
    }

    /// <summary>
    /// Moves an order one step forward in the fulfilment flow, or cancels it.
    /// Admin and staff have the same rights: an order can be cancelled until it is handed to the courier.
    /// </summary>
    public async Task UpdateOrderStatusAsync(
        int orderId,
        OrderStatus nextStatus,
        string? cancelReason,
        string actorName,
        CancellationToken cancellationToken = default)
    {
        var updated = await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var order = await _orderRepository.GetDetailsForManagementAsync(orderId, token)
                ?? throw new KeyNotFoundException("Không tìm thấy đơn hàng.");

            if (!GetAvailableStatuses(order.Status).Contains(nextStatus))
            {
                throw new InvalidOperationException("Không thể chuyển đơn hàng sang trạng thái đã chọn.");
            }

            if (order.PaymentMethod == PaymentMethod.VnPay
                && order.PaymentStatus != PaymentStatus.Paid
                && nextStatus != OrderStatus.Cancelled)
            {
                throw new InvalidOperationException("Đơn hàng VNPAY cần được thanh toán trước khi cửa hàng xử lý.");
            }

            var now = DateTime.UtcNow;
            if (nextStatus == OrderStatus.Cancelled)
            {
                await ReleaseReservedStockAsync(order, $"Hoàn kho do hủy đơn hàng {order.OrderCode}", now, token);
                AppendNote(order, string.IsNullOrWhiteSpace(cancelReason)
                    ? $"Cửa hàng đã hủy đơn ({actorName})."
                    : $"Cửa hàng đã hủy đơn ({actorName}). Lý do: {cancelReason.Trim()}");
            }

            // COD: the courier collects the money on delivery, so a delivered order is a paid order.
            if (nextStatus == OrderStatus.Completed && order.PaymentStatus != PaymentStatus.Paid)
            {
                order.PaymentStatus = PaymentStatus.Paid;
                order.PaidAt = now;
            }

            order.Status = nextStatus;
            order.UpdatedAt = now;
            return order;
        }, cancellationToken);

        // After commit: tell the customer (in-site, real time).
        await _notificationService.NotifyOrderStatusChangedAsync(
            updated.Id, updated.OrderCode, updated.UserId, updated.Status, updated.TotalAmount,
            nextStatus == OrderStatus.Cancelled ? cancelReason : null,
            cancellationToken);
    }

    #endregion

    #region Helpers

    private static OrderLineViewModel ToLine(OrderItem item) => new()
    {
        ProductName = item.ProductName,
        SKU = item.SKU,
        UnitPrice = item.UnitPrice,
        Quantity = item.Quantity,
        LineTotal = item.TotalPrice
    };

    private static OrderManagementRowViewModel ToManagementRow(Order order)
    {
        return new OrderManagementRowViewModel
        {
            Id = order.Id,
            OrderCode = order.OrderCode,
            CustomerName = order.RecipientName ?? order.User?.FullName ?? order.User?.UserName ?? "Khách hàng",
            CustomerPhone = order.RecipientPhone ?? string.Empty,
            CreatedAt = order.CreatedAt,
            Status = order.Status,
            StatusText = GetStatusText(order.Status),
            PaymentMethod = order.PaymentMethod,
            PaymentStatus = order.PaymentStatus,
            TotalAmount = order.TotalAmount
        };
    }

    private static string GetStatusText(OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Pending => "Chờ xác nhận",
            OrderStatus.Confirmed => "Đã xác nhận",
            OrderStatus.Processing => "Đang xử lý",
            OrderStatus.Shipping => "Đang giao",
            OrderStatus.Completed => "Đã giao thành công",
            OrderStatus.Cancelled => "Đã hủy",
            _ => "Không xác định"
        };
    }

    /// <summary>
    /// Next step of the fulfilment flow, plus cancellation while the order has not left the shop.
    /// </summary>
    private static IReadOnlyList<OrderStatus> GetAvailableStatuses(OrderStatus current) => current switch
    {
        OrderStatus.Pending => [OrderStatus.Confirmed, OrderStatus.Cancelled],
        OrderStatus.Confirmed => [OrderStatus.Processing, OrderStatus.Cancelled],
        OrderStatus.Processing => [OrderStatus.Shipping, OrderStatus.Cancelled],
        OrderStatus.Shipping => [OrderStatus.Completed],
        _ => []
    };

    /// <summary>A customer may cancel only an order the shop has not confirmed yet.</summary>
    private static bool CanCustomerCancel(Order order) => order.Status == OrderStatus.Pending;

    private static void AppendNote(Order order, string line)
    {
        var note = string.IsNullOrWhiteSpace(order.Note) ? line : $"{order.Note}\n{line}";
        order.Note = note.Length > 1000 ? note[..1000] : note;
    }

    private async Task ReleaseReservedStockAsync(
        Order order,
        string note,
        DateTime now,
        CancellationToken cancellationToken)
    {
        // StockReleasedAt makes the restock idempotent: an order never returns its items to stock twice.
        if (order.StockReleasedAt.HasValue)
        {
            return;
        }

        foreach (var item in order.OrderItems)
        {
            var variant = item.ProductVariant
                ?? throw new InvalidOperationException("Không tìm thấy phiên bản sản phẩm để hoàn tồn kho.");
            variant.StockQuantity += item.Quantity;
            await _inventoryTransactionRepository.AddAsync(new InventoryTransaction
            {
                ProductVariantId = variant.Id,
                Type = InventoryTransactionType.OrderCancelRestock,
                Quantity = item.Quantity,
                Note = note,
                CreatedAt = now
            }, cancellationToken);
        }

        order.StockReleasedAt = now;
    }

    #endregion
}
