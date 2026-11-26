using Microsoft.AspNetCore.Http;

namespace PopPop.Services.Interfaces;

public interface IVnPayPaymentService
{
    string CreatePaymentUrl(string transactionReference, decimal amount, string ipAddress, string returnUrl);
    bool ValidateSignature(IQueryCollection query);
}
