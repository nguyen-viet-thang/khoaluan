using PopPop.Data.Entities.Enums;

namespace PopPop.Data.Entities;

/// <summary>
/// A kind of defect customers can report for products of one category, with the evidence they must record.
/// </summary>
public class DefectType : IBaseEntity<int>
{
    public int Id { get; set; }

    /// <summary>Short code such as V01 (unique).</summary>
    public required string Code { get; set; }

    public int CategoryId { get; set; }
    public required string Name { get; set; }

    /// <summary>How to recognise the defect.</summary>
    public string? Recognition { get; set; }

    /// <summary>What the customer's video/photos must show.</summary>
    public string? EvidenceGuide { get; set; }

    public DefectCoverage Coverage { get; set; }

    /// <summary>Policy basis (brand policy, shop policy...).</summary>
    public string? Source { get; set; }
    public string? SourceUrl { get; set; }

    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public virtual Category? Category { get; set; }
}

/// <summary>
/// Manufacturer warranty length for a brand's products in one category. A missing row means no brand warranty.
/// </summary>
public class WarrantyPolicy : IBaseEntity<int>
{
    public int Id { get; set; }
    public int BrandId { get; set; }
    public int CategoryId { get; set; }

    /// <summary>Warranty length counted from delivery; 0 = no manufacturer warranty.</summary>
    public int WarrantyDays { get; set; }

    public string? CoveredParts { get; set; }
    public string? Notes { get; set; }
    public string? SourceUrl { get; set; }
    public DateTime UpdatedAt { get; set; }

    public virtual Brand? Brand { get; set; }
    public virtual Category? Category { get; set; }
}

/// <summary>
/// A customer's warranty / receipt-defect claim for one order line.
/// </summary>
public class WarrantyClaim : IBaseEntity<int>
{
    public int Id { get; set; }
    public required string Code { get; set; }
    public required string UserId { get; set; }
    public int OrderId { get; set; }
    public int OrderItemId { get; set; }
    public int DefectTypeId { get; set; }
    public ClaimType Type { get; set; }
    public ClaimStatus Status { get; set; } = ClaimStatus.Submitted;
    public int Quantity { get; set; } = 1;
    public required string Description { get; set; }

    /// <summary>Serial / authenticity sticker code printed on the product (rackets).</summary>
    public string? SerialNumber { get; set; }

    /// <summary>Last day the claim could be filed (receipt window or warranty end), kept for audit.</summary>
    public DateTime DeadlineAt { get; set; }

    public ClaimResolution? Resolution { get; set; }

    // ----- Customer → shop: sending the item in after approval -----
    public string? ReturnCarrier { get; set; }
    public string? ReturnTrackingCode { get; set; }
    public DateTime? ReturnShippedAt { get; set; }

    // ----- Shop → customer: replacement, or the original item when the claim is rejected after inspection -----
    public string? OutboundCarrier { get; set; }
    public string? OutboundTrackingCode { get; set; }
    public DateTime? OutboundShippedAt { get; set; }

    /// <summary>Internal note visible to staff only.</summary>
    public string? StaffNote { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }

    public virtual User? User { get; set; }
    public virtual Order? Order { get; set; }
    public virtual OrderItem? OrderItem { get; set; }
    public virtual DefectType? DefectType { get; set; }
    public virtual ICollection<WarrantyClaimMedia> Media { get; set; } = new List<WarrantyClaimMedia>();
    public virtual ICollection<WarrantyClaimHistory> History { get; set; } = new List<WarrantyClaimHistory>();
}

/// <summary>Evidence file (video/photo) attached to a claim. Stored outside wwwroot.</summary>
public class WarrantyClaimMedia : IBaseEntity<int>
{
    public int Id { get; set; }
    public int ClaimId { get; set; }
    public ClaimMediaKind Kind { get; set; }

    /// <summary>Random file name on disk.</summary>
    public required string StoredFileName { get; set; }
    public required string OriginalFileName { get; set; }
    public required string ContentType { get; set; }
    public long SizeBytes { get; set; }
    public required string UploadedByUserId { get; set; }
    public bool UploadedByStaff { get; set; }
    public DateTime UploadedAt { get; set; }

    public virtual WarrantyClaim? Claim { get; set; }
}

/// <summary>Audit trail: every status change or message on a claim.</summary>
public class WarrantyClaimHistory : IBaseEntity<int>
{
    public int Id { get; set; }
    public int ClaimId { get; set; }
    public ClaimStatus? FromStatus { get; set; }
    public ClaimStatus ToStatus { get; set; }
    public string? Note { get; set; }
    public required string ActorUserId { get; set; }
    public required string ActorName { get; set; }
    public bool ByStaff { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual WarrantyClaim? Claim { get; set; }
}
