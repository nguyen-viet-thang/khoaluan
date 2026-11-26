namespace PopPop.Data.Entities.Enums;

/// <summary>
/// Inventory transaction type enum
/// </summary>
public enum InventoryTransactionType
{
    /// <summary>
    /// Import inventory
    /// </summary>
    Import = 1,

    /// <summary>
    /// Sale transaction
    /// </summary>
    Sale = 2,

    /// <summary>
    /// Legacy manual adjustment (increase). No longer created; kept so old rows still map.
    /// </summary>
    AdjustmentIncrease = 3,

    /// <summary>
    /// Legacy manual adjustment (decrease). No longer created; kept so old rows still map.
    /// </summary>
    AdjustmentDecrease = 4,

    /// <summary>
    /// Stock issued to replace a customer's item under a warranty / receipt-defect claim
    /// </summary>
    ClaimReplacement = 5,

    /// <summary>
    /// Returned item that is still sellable (e.g. wrong item delivered) put back into stock
    /// </summary>
    ClaimRestock = 6,

    /// <summary>
    /// Items of a cancelled order put back into stock
    /// </summary>
    OrderCancelRestock = 7
}
