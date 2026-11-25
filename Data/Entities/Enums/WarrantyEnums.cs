namespace PopPop.Data.Entities.Enums;

/// <summary>Which policy a defect falls under.</summary>
public enum DefectCoverage
{
    /// <summary>Manufacturer warranty (brand policy, within the warranty period).</summary>
    Warranty = 1,

    /// <summary>Defect found on receipt / wrong item delivered (shop responsibility, within the brand's report window).</summary>
    ReceiptDefect = 2,

    /// <summary>Listed only to explain to customers why it is not accepted.</summary>
    NotCovered = 3
}

/// <summary>Type of a customer claim (derived from the chosen defect).</summary>
public enum ClaimType
{
    ReceiptDefect = 1,
    Warranty = 2
}

public enum ClaimStatus
{
    /// <summary>Waiting for staff review.</summary>
    Submitted = 1,

    /// <summary>Staff asked the customer for more evidence.</summary>
    NeedMoreInfo = 2,

    /// <summary>Accepted for inspection; the customer sends the item to the shop.</summary>
    Approved = 3,

    /// <summary>The shop received the item and inspects it.</summary>
    Received = 4,

    /// <summary>Forwarded to the brand/distributor for warranty assessment.</summary>
    SentToBrand = 5,

    Completed = 6,
    Rejected = 7,
    Cancelled = 8,

    /// <summary>The replacement (shop exchange or brand replacement) is on its way back to the customer.</summary>
    ReturningToCustomer = 9
}

public enum ClaimResolution
{
    /// <summary>The shop sends the correct/new item from its own stock.</summary>
    ExchangedByShop = 1,

    /// <summary>The brand replaced the product under warranty.</summary>
    ReplacedByBrand = 2

    // Shop policy: no refunds — a claim ends with a new product or a rejection.
}

public enum ClaimMediaKind
{
    Video = 1,
    Image = 2
}
