using Microsoft.AspNetCore.SignalR;
using PopPop.Data.Entities;
using PopPop.Data.Entities.Enums;
using PopPop.Hubs;
using PopPop.Models;
using PopPop.Repositories.Interfaces;
using PopPop.Services.Interfaces;

namespace PopPop.Services.Implementations;

public sealed class NotificationService : INotificationService
{
    private const int PageSize = 20;

    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        INotificationRepository notificationRepository,
        IUnitOfWork unitOfWork,
        IHubContext<NotificationHub> hubContext,
        ILogger<NotificationService> logger)
    {
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyClaimStatusChangedAsync(
        int claimId,
        string claimCode,
        string customerUserId,
        ClaimStatus newStatus,
        ClaimResolution? resolution,
        string? staffNote,
        CancellationToken cancellationToken = default)
    {
        var (title, message) = ComposeClaimMessage(claimCode, newStatus, resolution, staffNote);
        await NotifyCustomerAsync(customerUserId, title, message, $"/Claims/Details/{claimId}", cancellationToken);
    }

    public async Task NotifyClaimMessageAsync(
        int claimId,
        string claimCode,
        string customerUserId,
        string message,
        CancellationToken cancellationToken = default)
    {
        await NotifyCustomerAsync(customerUserId, $"Cửa hàng nhắn về yêu cầu {claimCode}", message.Trim(),
            $"/Claims/Details/{claimId}", cancellationToken);
    }

    public async Task NotifyOrderStatusChangedAsync(
        int orderId,
        string orderCode,
        string customerUserId,
        OrderStatus newStatus,
        decimal totalAmount,
        string? cancelReason,
        CancellationToken cancellationToken = default)
    {
        var (title, message) = ComposeOrderMessage(orderCode, newStatus, totalAmount, cancelReason);
        await NotifyCustomerAsync(customerUserId, title, message, $"/Orders/Details/{orderId}", cancellationToken);
    }

    /// <summary>
    /// Stores the notification and pushes it to the customer's open tabs (the shop does not use email).
    /// Never throws: the business action that triggered it has already been committed.
    /// </summary>
    private async Task NotifyCustomerAsync(
        string customerUserId,
        string title,
        string message,
        string url,
        CancellationToken cancellationToken)
    {
        try
        {
            // 1. In-site notification (stored, so the customer sees it even if they were offline).
            var notification = new Notification
            {
                UserId = customerUserId,
                Title = Truncate(title, 256)!,
                Message = Truncate(message, 1000),
                Url = url,
                CreatedAt = DateTime.UtcNow
            };
            await _notificationRepository.AddAsync(notification, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 2. Real-time push to every open tab of this customer.
            await _hubContext.Clients.User(customerUserId).SendAsync(
                NotificationHub.ReceivedEvent,
                new NotificationPush(notification.Id, notification.Title, notification.Message,
                    $"/Notifications/Open/{notification.Id}", ShopClock.ToLocal(notification.CreatedAt).ToString("dd/MM HH:mm")),
                cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogError(exception, "Could not create/push notification for user {UserId}: {Title}", customerUserId, title);
        }
    }

    public async Task NotifyStaffAsync(string title, string? message, string url, CancellationToken cancellationToken = default)
    {
        try
        {
            var staffIds = await _notificationRepository.GetActiveStaffUserIdsAsync(cancellationToken);
            if (staffIds.Count == 0)
            {
                return;
            }

            var now = DateTime.UtcNow;
            var notifications = staffIds.Select(userId => new Notification
            {
                UserId = userId,
                Title = Truncate(title, 256)!,
                Message = Truncate(message, 1000),
                Url = url,
                CreatedAt = now
            }).ToList();
            await _notificationRepository.AddRangeAsync(notifications, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Each recipient gets its own notification id (read state is per person).
            foreach (var notification in notifications)
            {
                await _hubContext.Clients.User(notification.UserId).SendAsync(
                    NotificationHub.ReceivedEvent,
                    new NotificationPush(notification.Id, notification.Title, notification.Message,
                        $"/Notifications/Open/{notification.Id}", ShopClock.ToLocal(now).ToString("dd/MM HH:mm")),
                    cancellationToken);
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            // The customer's action already succeeded; a notification problem must not turn it into an error page.
            _logger.LogError(exception, "Could not notify staff: {Title}", title);
        }
    }

    public async Task<NotificationListViewModel> GetPageAsync(string userId, int page, CancellationToken cancellationToken = default)
    {
        var total = await _notificationRepository.CountForUserAsync(userId, cancellationToken);
        var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
        page = Math.Clamp(page, 1, totalPages);
        var items = await _notificationRepository.GetForUserAsync(userId, (page - 1) * PageSize, PageSize, cancellationToken);
        return new NotificationListViewModel
        {
            Items = items.Select(ToItem).ToList(),
            UnreadCount = await _notificationRepository.CountUnreadAsync(userId, cancellationToken),
            PageNumber = page,
            TotalPages = totalPages
        };
    }

    public async Task<string?> OpenAsync(int notificationId, string userId, CancellationToken cancellationToken = default)
    {
        var notification = await _notificationRepository.GetByIdAsync(notificationId, cancellationToken);
        if (notification is null || notification.UserId != userId)
        {
            return null;
        }

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return notification.Url ?? "/Notifications";
    }

    public async Task MarkAllReadAsync(string userId, CancellationToken cancellationToken = default)
    {
        await _notificationRepository.MarkAllReadAsync(userId, DateTime.UtcNow, cancellationToken);
    }

    public static NotificationItemViewModel ToItem(Notification notification) => new()
    {
        Id = notification.Id,
        Title = notification.Title,
        Message = notification.Message,
        Url = notification.Url,
        IsRead = notification.IsRead,
        CreatedAt = notification.CreatedAt
    };

    /// <summary>Customer-facing wording for each status staff can set.</summary>
    private static (string Title, string Message) ComposeClaimMessage(
        string code, ClaimStatus status, ClaimResolution? resolution, string? note)
    {
        var (title, message) = status switch
        {
            ClaimStatus.NeedMoreInfo => ($"Yêu cầu {code} cần bổ sung minh chứng",
                "Cửa hàng cần thêm video/ảnh để xem xét yêu cầu của bạn. Vui lòng mở yêu cầu và gửi bổ sung."),
            ClaimStatus.Approved => ($"Yêu cầu {code} đã được duyệt",
                "Vui lòng gửi sản phẩm về cửa hàng theo hướng dẫn, giữ nguyên tình trạng (với vợt: không cắt dây), rồi nhập mã vận đơn trong trang yêu cầu."),
            ClaimStatus.ReturningToCustomer => ($"Cửa hàng đã gửi sản phẩm thay thế cho yêu cầu {code}",
                "Kết quả: " + (resolution is { } sent ? ClaimDisplay.ResolutionText(sent) : "đổi sản phẩm")
                + ". Khi nhận được hàng, vui lòng bấm \"Đã nhận hàng\" trong trang yêu cầu."),
            ClaimStatus.Received => ($"Cửa hàng đã nhận sản phẩm của yêu cầu {code}",
                "Sản phẩm đang được kiểm tra. Cửa hàng sẽ thông báo kết quả cho bạn."),
            ClaimStatus.SentToBrand => ($"Yêu cầu {code} đã được gửi hãng thẩm định",
                "Sản phẩm đã được chuyển tới hãng/nhà phân phối để thẩm định bảo hành. Thời gian thẩm định có thể kéo dài vài tuần."),
            ClaimStatus.Completed => ($"Yêu cầu {code} đã hoàn tất",
                "Kết quả: " + (resolution is { } value ? ClaimDisplay.ResolutionText(value) : "đã xử lý") + "."),
            ClaimStatus.Rejected => ($"Yêu cầu {code} không được chấp nhận",
                "Cửa hàng không thể tiếp nhận yêu cầu này."),
            _ => ($"Yêu cầu {code}: {ClaimDisplay.StatusText(status)}", string.Empty)
        };

        return string.IsNullOrWhiteSpace(note) ? (title, message) : (title, $"{message} Lời nhắn của cửa hàng: {note.Trim()}");
    }

    /// <summary>Customer-facing wording for each order status staff can set.</summary>
    private static (string Title, string Message) ComposeOrderMessage(
        string code, OrderStatus status, decimal totalAmount, string? cancelReason) => status switch
    {
        OrderStatus.Confirmed => ($"Đơn hàng {code} đã được xác nhận",
            "Cửa hàng đã xác nhận đơn hàng và sẽ chuẩn bị hàng cho bạn."),
        OrderStatus.Processing => ($"Đơn hàng {code} đang được chuẩn bị",
            "Sản phẩm đang được kiểm tra và đóng gói."),
        OrderStatus.Shipping => ($"Đơn hàng {code} đang được giao",
            $"Đơn hàng đã giao cho đơn vị vận chuyển. Vui lòng giữ điện thoại để nhân viên giao hàng liên hệ và chuẩn bị {totalAmount:N0} ₫ thanh toán khi nhận hàng (COD)."),
        OrderStatus.Completed => ($"Đơn hàng {code} đã giao thành công",
            "Cảm ơn bạn đã mua sắm tại PôpPôp! Nếu sản phẩm bị lỗi khi nhận hàng hoặc gặp lỗi thuộc diện bảo hành, bạn có thể gửi yêu cầu kèm video trong trang chi tiết đơn hàng."),
        OrderStatus.Cancelled => ($"Đơn hàng {code} đã bị hủy",
            "Cửa hàng đã hủy đơn hàng của bạn" + (string.IsNullOrWhiteSpace(cancelReason) ? "." : $". Lý do: {cancelReason.Trim()}.")
            + " Vui lòng liên hệ cửa hàng nếu bạn cần hỗ trợ."),
        _ => ($"Đơn hàng {code}: cập nhật trạng thái", string.Empty)
    };

    private static string? Truncate(string? value, int max) => value is { Length: > 0 } && value.Length > max ? value[..max] : value;
}
