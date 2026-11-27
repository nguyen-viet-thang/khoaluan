namespace PopPop.Models;

public sealed class VnPayOptions
{
    public const string SectionName = "Payment:VnPay";

    public string TmnCode { get; set; } = string.Empty;
    public string HashSecret { get; set; } = string.Empty;
    public string PaymentUrl { get; set; } = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";

    /// <summary>
    /// Minutes the customer has to pay a VNPAY order after placing it. After that (plus a short grace period for
    /// late IPN calls) the unpaid order is cancelled automatically and its items go back to stock.
    /// </summary>
    public int PaymentTimeoutMinutes { get; set; } = 30;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(TmnCode) && !string.IsNullOrWhiteSpace(HashSecret);

    public TimeSpan PaymentTimeout => TimeSpan.FromMinutes(Math.Clamp(PaymentTimeoutMinutes, 5, 24 * 60));
}
