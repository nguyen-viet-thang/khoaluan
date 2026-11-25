namespace PopPop.Data.Entities.Enums;

public enum PaymentStatus
{
    Unpaid = 1,

    /// <summary>Money collected (for COD: set when the order is delivered).</summary>
    Paid = 3
}
