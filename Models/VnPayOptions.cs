namespace PopPop.Models;

public sealed class VnPayOptions
{
    public const string SectionName = "Payment:VnPay";

    public string TmnCode { get; set; } = string.Empty;
    public string HashSecret { get; set; } = string.Empty;
    public string PaymentUrl { get; set; } = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
}
