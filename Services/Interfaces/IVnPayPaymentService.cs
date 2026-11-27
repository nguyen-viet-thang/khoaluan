using Microsoft.AspNetCore.Http;

namespace PopPop.Services.Interfaces;

/// <summary>Result of a VNPAY callback (return URL or IPN) after its signature was checked.</summary>
public sealed record VnPayCallback(
    bool IsValid,
    int OrderId,
    long Amount,
    bool Succeeded,
    string ResponseCode,
    string? TransactionNo);

public interface IVnPayPaymentService
{
    /// <summary>False when TmnCode/HashSecret are missing: checkout then offers COD only.</summary>
    bool IsConfigured { get; }

    /// <summary>How long a customer has to pay a VNPAY order after placing it.</summary>
    TimeSpan PaymentTimeout { get; }

    /// <summary>
    /// Builds the signed VNPAY payment URL. Every call uses a new transaction reference (VNPAY rejects a reused one),
    /// so the customer can retry after a failed or abandoned attempt.
    /// </summary>
    string CreatePaymentUrl(int orderId, string orderCode, decimal amount, string ipAddress, string returnUrl, DateTime expiresAtUtc);

    /// <summary>Validates the signature and reads the fields the shop needs. <see cref="VnPayCallback.IsValid"/> is false on any problem.</summary>
    VnPayCallback ParseCallback(IQueryCollection query);

    /// <summary>Vietnamese explanation of a vnp_ResponseCode for the customer.</summary>
    string DescribeResponseCode(string responseCode);
}
