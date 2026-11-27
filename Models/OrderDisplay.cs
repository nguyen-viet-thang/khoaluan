using PopPop.Data.Entities.Enums;

namespace PopPop.Models;

/// <summary>Display texts and CSS classes for order statuses in the back office (Admin and Staff share them).</summary>
public static class OrderDisplay
{
    public static string StatusText(OrderStatus status) => status switch
    {
        OrderStatus.Pending => "Chờ xác nhận",
        OrderStatus.Confirmed => "Đã xác nhận",
        OrderStatus.Processing => "Đang xử lý",
        OrderStatus.Shipping => "Đang giao",
        OrderStatus.Completed => "Đã giao thành công",
        OrderStatus.Cancelled => "Đã hủy",
        _ => "Không xác định"
    };

    public static string StatusClass(OrderStatus status) => status switch
    {
        OrderStatus.Pending => "status-pending",
        OrderStatus.Confirmed => "status-confirmed",
        OrderStatus.Processing => "status-processing",
        OrderStatus.Shipping => "status-shipping",
        OrderStatus.Completed => "status-completed",
        OrderStatus.Cancelled => "status-cancelled",
        _ => string.Empty
    };

    /// <summary>Button text for moving an order to <paramref name="next"/>.</summary>
    public static string ActionText(OrderStatus next) => next switch
    {
        OrderStatus.Confirmed => "Xác nhận",
        OrderStatus.Processing => "Chuẩn bị hàng",
        OrderStatus.Shipping => "Giao vận chuyển",
        OrderStatus.Completed => "Đã giao xong",
        OrderStatus.Cancelled => "Hủy đơn",
        _ => StatusText(next)
    };

    public static string ActionIcon(OrderStatus next) => next switch
    {
        OrderStatus.Confirmed => "fa-check",
        OrderStatus.Processing => "fa-archive",
        OrderStatus.Shipping => "fa-truck",
        OrderStatus.Completed => "fa-flag-checkered",
        OrderStatus.Cancelled => "fa-times",
        _ => "fa-arrow-right"
    };

    /// <summary>"5 phút trước", "3 giờ trước", "2 ngày trước".</summary>
    public static string Ago(DateTime utc)
    {
        var elapsed = DateTime.UtcNow - utc;
        return elapsed.TotalMinutes < 1 ? "vừa xong"
            : elapsed.TotalHours < 1 ? $"{(int)elapsed.TotalMinutes} phút trước"
            : elapsed.TotalDays < 1 ? $"{(int)elapsed.TotalHours} giờ trước"
            : $"{(int)elapsed.TotalDays} ngày trước";
    }
}
