using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using PopPop.Data.Entities.Enums;

namespace PopPop.Models;

// ===================== Customer: create a claim =====================

/// <summary>An order line the customer may file a claim for, with its deadlines.</summary>
public sealed class ClaimItemOptionViewModel
{
    public int OrderItemId { get; init; }
    public required string ProductName { get; init; }
    public required string SKU { get; init; }
    public string? ImageUrl { get; init; }
    public int CategoryId { get; init; }
    public required string CategoryName { get; init; }
    public required string BrandName { get; init; }
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public int ReceiptDefectDays { get; init; }
    public DateTime ReceiptDeadline { get; init; }
    public int WarrantyDays { get; init; }
    public DateTime? WarrantyDeadline { get; init; }
    public bool ReceiptWindowOpen { get; init; }
    public bool WarrantyWindowOpen { get; init; }
    /// <summary>Code of an open claim for this line; a line can have only one open claim.</summary>
    public string? OpenClaimCode { get; init; }

    /// <summary>A new claim can be filed now (no open claim, and the receipt or warranty window is still open).</summary>
    public bool CanClaim => OpenClaimCode is null && (ReceiptWindowOpen || WarrantyWindowOpen);
}

/// <summary>A delivered order on the "start a claim" page, with its lines and their claim deadlines.</summary>
public sealed class ClaimOrderOptionViewModel
{
    public int OrderId { get; init; }
    public required string OrderCode { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime DeliveredAt { get; init; }
    public decimal TotalAmount { get; init; }
    public string? RecipientName { get; init; }
    public string? RecipientPhone { get; init; }
    public string? ShippingAddress { get; init; }
    public IReadOnlyList<ClaimItemOptionViewModel> Items { get; init; } = [];
}

/// <summary>Step 1 of a claim: the customer picks a product from their delivered orders.</summary>
public sealed class ClaimStartViewModel
{
    public IReadOnlyList<ClaimOrderOptionViewModel> Orders { get; init; } = [];
    public string? Search { get; init; }
    /// <summary>Orders still being processed/shipped: they can be claimed once delivered.</summary>
    public int UndeliveredOrderCount { get; init; }
}

public sealed class DefectOptionViewModel
{
    public int Id { get; init; }
    public required string Code { get; init; }
    public int CategoryId { get; init; }
    public required string Name { get; init; }
    public string? Recognition { get; init; }
    public string? EvidenceGuide { get; init; }
    public DefectCoverage Coverage { get; init; }
    public string? Source { get; init; }
    public string? SourceUrl { get; init; }
}

public sealed class CreateClaimViewModel
{
    public int OrderId { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn sản phẩm cần bảo hành/đổi.")]
    public int? OrderItemId { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn loại lỗi.")]
    public int? DefectTypeId { get; set; }

    [Range(1, 1000, ErrorMessage = "Số lượng không hợp lệ.")]
    public int Quantity { get; set; } = 1;

    [StringLength(100)]
    [Display(Name = "Số serial / mã tem chính hãng")]
    public string? SerialNumber { get; set; }

    [Required(ErrorMessage = "Vui lòng mô tả tình trạng sản phẩm.")]
    [StringLength(2000, MinimumLength = 20, ErrorMessage = "Mô tả cần từ 20 đến 2000 ký tự để cửa hàng nắm rõ tình trạng.")]
    [Display(Name = "Mô tả nguyên nhân / tình trạng lỗi")]
    public string Description { get; set; } = string.Empty;

    /// <summary>The product was picked on step 1: show only that product instead of the order's list.</summary>
    public bool ProductLocked { get; set; }

    [ValidateNever] public string OrderCode { get; set; } = string.Empty;
    [ValidateNever] public DateTime DeliveredAt { get; set; }
    [ValidateNever] public IReadOnlyList<ClaimItemOptionViewModel> Items { get; set; } = [];
    [ValidateNever] public IReadOnlyList<DefectOptionViewModel> Defects { get; set; } = [];
    [ValidateNever] public ClaimMediaLimits Limits { get; set; } = new();
}

/// <summary>Upload limits shown to the customer and enforced by the server.</summary>
public sealed class ClaimMediaLimits
{
    public int MaxVideos { get; init; } = 3;
    public long MaxVideoBytes { get; init; } = 100L * 1024 * 1024;
    public int MaxImages { get; init; } = 5;
    public long MaxImageBytes { get; init; } = 5L * 1024 * 1024;
    public string VideoExtensions { get; init; } = ".mp4,.mov,.webm";
    public string ImageExtensions { get; init; } = ".jpg,.jpeg,.png,.webp";
}

// ===================== Shared: list & details =====================

public sealed class ClaimRowViewModel
{
    public int Id { get; init; }
    public required string Code { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public required string ProductName { get; init; }
    public required string DefectName { get; init; }
    public ClaimType Type { get; init; }
    public ClaimStatus Status { get; init; }
    public string? CustomerName { get; init; }
    public string? OrderCode { get; init; }
}

public sealed class ClaimListViewModel
{
    public IReadOnlyList<ClaimRowViewModel> Claims { get; init; } = [];
    public ClaimStatus? Status { get; init; }
    public ClaimType? Type { get; init; }
    public string? Search { get; init; }
    public int OpenCount { get; init; }
}

public sealed class ClaimMediaViewModel
{
    public int Id { get; init; }
    public ClaimMediaKind Kind { get; init; }
    public required string OriginalFileName { get; init; }
    public long SizeBytes { get; init; }
    public bool UploadedByStaff { get; init; }
    public DateTime UploadedAt { get; init; }
}

public sealed class ClaimHistoryViewModel
{
    public ClaimStatus? FromStatus { get; init; }
    public ClaimStatus ToStatus { get; init; }
    public string? Note { get; init; }
    public required string ActorName { get; init; }
    public bool ByStaff { get; init; }
    public DateTime CreatedAt { get; init; }

    /// <summary>Message / information update that did not change the status.</summary>
    public bool IsUpdate => FromStatus == ToStatus;
}

public sealed class ClaimDetailsViewModel
{
    public int Id { get; init; }
    public required string Code { get; init; }
    public ClaimType Type { get; init; }
    public ClaimStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime DeadlineAt { get; init; }
    public DateTime? ClosedAt { get; init; }

    public int OrderId { get; init; }
    public required string OrderCode { get; init; }
    public DateTime DeliveredAt { get; init; }
    public required string ProductName { get; init; }
    public required string SKU { get; init; }
    public int ProductVariantId { get; init; }
    public decimal UnitPrice { get; init; }
    public int Quantity { get; init; }

    public required string DefectCode { get; init; }
    public required string DefectName { get; init; }
    public string? EvidenceGuide { get; init; }
    public required string Description { get; init; }
    public string? SerialNumber { get; init; }

    public string? CustomerName { get; init; }
    /// <summary>Phone number the customer account signs in with (staff only).</summary>
    public string? CustomerAccountPhone { get; init; }
    public string? CustomerPhone { get; init; }

    public ClaimResolution? Resolution { get; init; }
    /// <summary>Staff-only internal note (null for customers).</summary>
    public string? StaffNote { get; init; }

    // Shipping in both directions
    public string? ReturnCarrier { get; init; }
    public string? ReturnTrackingCode { get; init; }
    public DateTime? ReturnShippedAt { get; init; }
    public string? OutboundCarrier { get; init; }
    public string? OutboundTrackingCode { get; init; }
    public DateTime? OutboundShippedAt { get; init; }
    /// <summary>Where the replacement / returned item is sent (the order's delivery address).</summary>
    public string? DeliveryName { get; init; }
    public string? DeliveryPhone { get; init; }
    public string? DeliveryAddress { get; init; }
    /// <summary>Shop address/hotline from the Store config (null when not configured).</summary>
    public string? ShopAddress { get; init; }
    public string? ShopHotline { get; init; }

    public IReadOnlyList<ClaimMediaViewModel> Media { get; init; } = [];
    public IReadOnlyList<ClaimHistoryViewModel> History { get; init; } = [];

    // Customer actions
    public bool CanCancel { get; init; }
    public bool CanAddEvidence { get; init; }
    public bool CanReportShipment { get; init; }
    public bool CanConfirmReceived { get; init; }

    // Staff actions
    public IReadOnlyList<ClaimStatus> AllowedNextStatuses { get; init; } = [];
    /// <summary>The customer's item is physically at the shop (or the brand): rejecting must send it back.</summary>
    public bool ItemAtShop { get; init; }
    public bool CanMessage { get; init; }
    public int AvailableStock { get; init; }
    public ClaimMediaLimits Limits { get; init; } = new();
}

/// <summary>Staff decision posted from the claim details page.</summary>
public sealed class ClaimDecisionViewModel
{
    public ClaimStatus NextStatus { get; set; }
    public string? Note { get; set; }
    public ClaimResolution? Resolution { get; set; }
    /// <summary>For a receipt defect: put the returned item back into sellable stock (e.g. wrong item delivered).</summary>
    public bool RestockReturnedItem { get; set; }
    /// <summary>Shipment back to the customer (replacement, or the original item after a rejection).</summary>
    public string? OutboundCarrier { get; set; }
    public string? OutboundTrackingCode { get; set; }
}

/// <summary>Customer reports how the item was sent to the shop.</summary>
public sealed class ClaimShipmentInput
{
    [Required(ErrorMessage = "Vui lòng chọn hoặc nhập đơn vị vận chuyển.")]
    [StringLength(100)]
    public string Carrier { get; set; } = string.Empty;

    [StringLength(100)]
    public string? TrackingCode { get; set; }
}

// ===================== Admin: policy configuration =====================

public sealed class WarrantyPolicyMatrixViewModel
{
    public IReadOnlyList<PolicyBrandColumn> Brands { get; init; } = [];
    public IReadOnlyList<PolicyCategoryRow> Categories { get; init; } = [];
    public IReadOnlyList<WarrantyPolicyCell> Cells { get; init; } = [];
    public IReadOnlyList<DefectAdminRow> Defects { get; init; } = [];

    public WarrantyPolicyCell? Cell(int brandId, int categoryId) =>
        Cells.FirstOrDefault(cell => cell.BrandId == brandId && cell.CategoryId == categoryId);
}

public sealed record PolicyBrandColumn(int Id, string Name, int ReceiptDefectDays);
public sealed record PolicyCategoryRow(int Id, string Name);

public sealed class WarrantyPolicyCell
{
    public int BrandId { get; init; }
    public int CategoryId { get; init; }
    public int WarrantyDays { get; init; }
    public string? CoveredParts { get; init; }
    public string? Notes { get; init; }
    public string? SourceUrl { get; init; }
    public int ProductCount { get; init; }
}

public sealed class WarrantyPolicyFormViewModel
{
    public int BrandId { get; set; }
    public int CategoryId { get; set; }
    [ValidateNever] public string BrandName { get; set; } = string.Empty;
    [ValidateNever] public string CategoryName { get; set; } = string.Empty;

    [Range(0, 3650, ErrorMessage = "Thời hạn bảo hành phải từ 0 đến 3650 ngày.")]
    [Display(Name = "Thời hạn bảo hành (ngày, 0 = không bảo hành)")]
    public int WarrantyDays { get; set; }

    [StringLength(256)]
    [Display(Name = "Bộ phận được bảo hành")]
    public string? CoveredParts { get; set; }

    [StringLength(500)]
    [Display(Name = "Ghi chú / căn cứ")]
    public string? Notes { get; set; }

    [StringLength(500)]
    [Url(ErrorMessage = "Đường dẫn không hợp lệ.")]
    [Display(Name = "Link chính sách của hãng")]
    public string? SourceUrl { get; set; }
}

public sealed class DefectAdminRow
{
    public int Id { get; init; }
    public required string Code { get; init; }
    public required string CategoryName { get; init; }
    public required string Name { get; init; }
    public DefectCoverage Coverage { get; init; }
    public string? Source { get; init; }
    public bool IsActive { get; init; }
}

public sealed class DefectTypeFormViewModel
{
    public int Id { get; set; }
    [ValidateNever] public string Code { get; set; } = string.Empty;
    [ValidateNever] public string CategoryName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập tên lỗi.")]
    [StringLength(256)]
    [Display(Name = "Tên lỗi")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "Dấu hiệu nhận biết")]
    public string? Recognition { get; set; }

    [StringLength(500)]
    [Display(Name = "Hướng dẫn quay video/ảnh")]
    public string? EvidenceGuide { get; set; }

    [Display(Name = "Diện áp dụng")]
    public DefectCoverage Coverage { get; set; }

    [StringLength(256)]
    [Display(Name = "Căn cứ")]
    public string? Source { get; set; }

    [StringLength(500)]
    [Url(ErrorMessage = "Đường dẫn không hợp lệ.")]
    [Display(Name = "Link nguồn")]
    public string? SourceUrl { get; set; }

    [Display(Name = "Đang sử dụng")]
    public bool IsActive { get; set; } = true;
}
