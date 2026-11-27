using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using PopPop.Models;
using PopPop.Services.Interfaces;

namespace PopPop.Services.Implementations;

public sealed class VnPayPaymentService : IVnPayPaymentService
{
    /// <summary>Separates the order id from the per-attempt suffix in vnp_TxnRef, e.g. "125P20261127103000".</summary>
    private const char ReferenceSeparator = 'P';

    private static readonly TimeSpan MaxLinkLifetime = TimeSpan.FromMinutes(15);
    private static readonly TimeZoneInfo VietnamTime = ShopClock.TimeZone;

    private readonly VnPayOptions _options;

    public VnPayPaymentService(IOptions<VnPayOptions> options)
    {
        _options = options.Value;
    }

    public bool IsConfigured => _options.IsConfigured;

    public TimeSpan PaymentTimeout => _options.PaymentTimeout;

    public string CreatePaymentUrl(int orderId, string orderCode, decimal amount, string ipAddress, string returnUrl, DateTime expiresAtUtc)
    {
        if (!IsConfigured)
        {
            throw new InvalidOperationException("Cổng thanh toán VNPAY chưa được cấu hình. Vui lòng chọn thanh toán khi nhận hàng.");
        }

        var nowUtc = DateTime.UtcNow;
        // The link never outlives the order's payment deadline, so a late payment cannot hit an auto-cancelled order.
        var linkExpiresUtc = expiresAtUtc < nowUtc + MaxLinkLifetime ? expiresAtUtc : nowUtc + MaxLinkLifetime;
        var now = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, VietnamTime);
        var expires = TimeZoneInfo.ConvertTimeFromUtc(linkExpiresUtc, VietnamTime);
        var reference = string.Create(CultureInfo.InvariantCulture, $"{orderId}{ReferenceSeparator}{now:yyyyMMddHHmmss}");

        var parameters = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["vnp_Version"] = "2.1.0",
            ["vnp_Command"] = "pay",
            ["vnp_TmnCode"] = _options.TmnCode,
            ["vnp_Amount"] = ToVnPayAmount(amount).ToString(CultureInfo.InvariantCulture),
            ["vnp_CurrCode"] = "VND",
            ["vnp_TxnRef"] = reference,
            // VNPAY requires plain ASCII here (no Vietnamese diacritics).
            ["vnp_OrderInfo"] = $"Thanh toan don hang {orderCode}",
            ["vnp_OrderType"] = "other",
            ["vnp_Locale"] = "vn",
            ["vnp_ReturnUrl"] = returnUrl,
            ["vnp_IpAddr"] = ipAddress,
            ["vnp_CreateDate"] = now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture),
            ["vnp_ExpireDate"] = expires.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture)
        };

        var query = BuildHashData(parameters);
        return $"{_options.PaymentUrl}?{query}&vnp_SecureHash={ComputeSignature(query)}";
    }

    public VnPayCallback ParseCallback(IQueryCollection query)
    {
        var invalid = new VnPayCallback(false, 0, 0, false, "99", null);
        if (!ValidateSignature(query))
        {
            return invalid with { ResponseCode = "97" };
        }

        var reference = Single(query, "vnp_TxnRef");
        var separator = reference?.IndexOf(ReferenceSeparator) ?? -1;
        var orderIdText = separator > 0 ? reference![..separator] : reference;
        if (!int.TryParse(orderIdText, NumberStyles.None, CultureInfo.InvariantCulture, out var orderId)
            || !long.TryParse(Single(query, "vnp_Amount"), NumberStyles.None, CultureInfo.InvariantCulture, out var amount))
        {
            return invalid;
        }

        var responseCode = Single(query, "vnp_ResponseCode") ?? "99";
        var transactionStatus = Single(query, "vnp_TransactionStatus");
        return new VnPayCallback(
            true,
            orderId,
            amount,
            responseCode == "00" && transactionStatus == "00",
            responseCode,
            Single(query, "vnp_TransactionNo"));
    }

    public string DescribeResponseCode(string responseCode) => responseCode switch
    {
        "00" => "Giao dịch thành công.",
        "07" => "Giao dịch bị nghi ngờ gian lận, vui lòng liên hệ ngân hàng.",
        "09" => "Thẻ/tài khoản chưa đăng ký dịch vụ Internet Banking.",
        "10" => "Bạn đã xác thực thông tin thẻ/tài khoản sai quá 3 lần.",
        "11" => "Đã hết thời gian chờ thanh toán.",
        "12" => "Thẻ/tài khoản đang bị khóa.",
        "13" => "Mã OTP không chính xác.",
        "24" => "Bạn đã hủy giao dịch.",
        "51" => "Tài khoản không đủ số dư.",
        "65" => "Tài khoản đã vượt hạn mức giao dịch trong ngày.",
        "75" => "Ngân hàng thanh toán đang bảo trì.",
        "79" => "Bạn đã nhập sai mật khẩu thanh toán quá số lần quy định.",
        "97" => "Không xác thực được chữ ký dữ liệu từ VNPAY.",
        _ => "Giao dịch không thành công."
    };

    internal static long ToVnPayAmount(decimal amount) => checked((long)(amount * 100m));

    private bool ValidateSignature(IQueryCollection query)
    {
        if (!IsConfigured
            || !string.Equals(Single(query, "vnp_TmnCode"), _options.TmnCode, StringComparison.Ordinal)
            || Single(query, "vnp_SecureHash") is not { } suppliedHash)
        {
            return false;
        }

        var parameters = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var (key, values) in query)
        {
            if (!key.StartsWith("vnp_", StringComparison.Ordinal)
                || key is "vnp_SecureHash" or "vnp_SecureHashType")
            {
                continue;
            }

            if (values.Count != 1)
            {
                return false;
            }

            parameters[key] = values[0] ?? string.Empty;
        }

        try
        {
            var expectedHash = Convert.FromHexString(ComputeSignature(BuildHashData(parameters)));
            var actualHash = Convert.FromHexString(suppliedHash);
            return expectedHash.Length == actualHash.Length
                && CryptographicOperations.FixedTimeEquals(expectedHash, actualHash);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static string? Single(IQueryCollection query, string key) =>
        query.TryGetValue(key, out var values) && values.Count == 1 ? values[0] : null;

    private string ComputeSignature(string data)
    {
        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(_options.HashSecret));
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(data)));
    }

    private static string BuildHashData(IEnumerable<KeyValuePair<string, string>> parameters) =>
        string.Join("&", parameters.Select(parameter =>
            $"{WebUtility.UrlEncode(parameter.Key)}={WebUtility.UrlEncode(parameter.Value)}"));
}
