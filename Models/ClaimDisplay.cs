using PopPop.Data.Entities.Enums;

namespace PopPop.Models;

/// <summary>Vietnamese labels and CSS classes for warranty claims, shared by customer and staff views.</summary>
public static class ClaimDisplay
{
    public static string StatusText(ClaimStatus status) => status switch
    {
        ClaimStatus.Submitted => "Chờ duyệt",
        ClaimStatus.NeedMoreInfo => "Cần bổ sung minh chứng",
        ClaimStatus.Approved => "Đã duyệt – chờ gửi hàng về",
        ClaimStatus.Received => "Cửa hàng đã nhận hàng",
        ClaimStatus.SentToBrand => "Đang gửi hãng thẩm định",
        ClaimStatus.ReturningToCustomer => "Đang gửi hàng cho khách",
        ClaimStatus.Completed => "Hoàn tất",
        ClaimStatus.Rejected => "Từ chối",
        ClaimStatus.Cancelled => "Khách đã hủy",
        _ => status.ToString()
    };

    /// <summary>Statuses in workflow order (for filters).</summary>
    public static IReadOnlyList<ClaimStatus> OrderedStatuses { get; } =
    [
        ClaimStatus.Submitted, ClaimStatus.NeedMoreInfo, ClaimStatus.Approved, ClaimStatus.Received,
        ClaimStatus.SentToBrand, ClaimStatus.ReturningToCustomer, ClaimStatus.Completed, ClaimStatus.Rejected, ClaimStatus.Cancelled
    ];

    /// <summary>Suggested carriers (free text is still accepted).</summary>
    public static IReadOnlyList<string> Carriers { get; } =
        ["Giao Hàng Nhanh (GHN)", "Giao Hàng Tiết Kiệm (GHTK)", "Viettel Post", "J&T Express", "VNPost", "Ninja Van", "SPX Express"];

    /// <summary>Carrier value meaning "handed over in person at the shop" (no tracking code needed).</summary>
    public const string InPerson = "Trực tiếp tại cửa hàng";

    public static bool IsInPerson(string? carrier) => string.Equals(carrier?.Trim(), InPerson, StringComparison.OrdinalIgnoreCase);

    public static string ShipmentText(string? carrier, string? trackingCode) =>
        IsInPerson(carrier) ? InPerson.ToLowerInvariant()
        : string.IsNullOrWhiteSpace(trackingCode) ? $"qua {carrier}" : $"qua {carrier}, mã vận đơn {trackingCode}";
    /// <summary>Label of the staff action that moves a claim into the given status.</summary>
    public static string ActionText(ClaimStatus status) => status switch
    {
        ClaimStatus.NeedMoreInfo => "Yêu cầu khách bổ sung minh chứng",
        ClaimStatus.Approved => "Duyệt – hướng dẫn khách gửi hàng về",
        ClaimStatus.Received => "Đã nhận được hàng khách gửi",
        ClaimStatus.SentToBrand => "Gửi hãng thẩm định bảo hành",
        ClaimStatus.ReturningToCustomer => "Gửi sản phẩm thay thế cho khách",
        ClaimStatus.Completed => "Hoàn tất xử lý",
        ClaimStatus.Rejected => "Từ chối",
        _ => StatusText(status)
    };

    public static string StatusClass(ClaimStatus status) => status switch
    {
        ClaimStatus.Submitted => "status-pending",
        ClaimStatus.NeedMoreInfo => "status-pending",
        ClaimStatus.Approved or ClaimStatus.Received => "status-confirmed",
        ClaimStatus.SentToBrand or ClaimStatus.ReturningToCustomer => "status-shipping",
        ClaimStatus.Completed => "status-completed",
        _ => "status-cancelled"
    };

    public static string TypeText(ClaimType type) => type == ClaimType.Warranty ? "Bảo hành" : "Lỗi khi nhận hàng";

    public static string CoverageText(DefectCoverage coverage) => coverage switch
    {
        DefectCoverage.Warranty => "Bảo hành theo hãng",
        DefectCoverage.ReceiptDefect => "Lỗi khi nhận hàng",
        _ => "Không được chấp nhận"
    };

    public static string ResolutionText(ClaimResolution resolution) => resolution switch
    {
        ClaimResolution.ExchangedByShop => "Cửa hàng đổi sản phẩm mới",
        ClaimResolution.ReplacedByBrand => "Hãng thay thế sản phẩm",
        _ => resolution.ToString()
    };

    public static bool IsClosed(ClaimStatus status) => status is ClaimStatus.Completed or ClaimStatus.Rejected or ClaimStatus.Cancelled;

    public static string FileSize(long bytes) => bytes >= 1024 * 1024 ? $"{bytes / 1024d / 1024d:0.#} MB" : $"{bytes / 1024d:0} KB";
}
