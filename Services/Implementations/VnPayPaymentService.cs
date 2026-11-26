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
    private readonly VnPayOptions _options;

    public VnPayPaymentService(IOptions<VnPayOptions> options)
    {
        _options = options.Value;
    }

    public string CreatePaymentUrl(string transactionReference, decimal amount, string ipAddress, string returnUrl)
    {
        if (string.IsNullOrWhiteSpace(_options.TmnCode) || string.IsNullOrWhiteSpace(_options.HashSecret))
        {
            throw new InvalidOperationException("Thiếu cấu hình VNPAY TmnCode hoặc HashSecret trong User Secrets.");
        }

        var now = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7));
        var parameters = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["vnp_Version"] = "2.1.0",
            ["vnp_Command"] = "pay",
            ["vnp_TmnCode"] = _options.TmnCode,
            ["vnp_Amount"] = checked((long)(amount * 100m)).ToString(CultureInfo.InvariantCulture),
            ["vnp_CurrCode"] = "VND",
            ["vnp_TxnRef"] = transactionReference,
            ["vnp_OrderInfo"] = $"Thanh toan don hang {transactionReference}",
            ["vnp_OrderType"] = "other",
            ["vnp_Locale"] = "vn",
            ["vnp_ReturnUrl"] = returnUrl,
            ["vnp_IpAddr"] = ipAddress,
            ["vnp_CreateDate"] = now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture),
            ["vnp_ExpireDate"] = now.AddMinutes(15).ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture)
        };

        var signature = ComputeSignature(BuildHashData(parameters));
        var query = string.Join("&", parameters.Select(parameter =>
            $"{WebUtility.UrlEncode(parameter.Key)}={WebUtility.UrlEncode(parameter.Value)}"));
        return $"{_options.PaymentUrl}?{query}&vnp_SecureHash={signature}";
    }

    public bool ValidateSignature(IQueryCollection query)
    {
        if (string.IsNullOrWhiteSpace(_options.HashSecret)
            || !query.TryGetValue("vnp_TmnCode", out var merchantCode)
            || merchantCode.Count != 1
            || !string.Equals(merchantCode[0], _options.TmnCode, StringComparison.Ordinal)
            || !query.TryGetValue("vnp_SecureHash", out var suppliedHash)
            || suppliedHash.Count != 1)
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
            var actualHash = Convert.FromHexString(suppliedHash[0]!);
            return expectedHash.Length == actualHash.Length
                && CryptographicOperations.FixedTimeEquals(expectedHash, actualHash);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private string ComputeSignature(string data)
    {
        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(_options.HashSecret));
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(data)));
    }

    private static string BuildHashData(IEnumerable<KeyValuePair<string, string>> parameters) =>
        string.Join("&", parameters.Select(parameter =>
            $"{WebUtility.UrlEncode(parameter.Key)}={WebUtility.UrlEncode(parameter.Value)}"));
}
