using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Options;
using PopPop.Models;
using PopPop.Services.Interfaces;

namespace PopPop.Services.Implementations;

/// <summary>Sends mail through the SMTP server in <see cref="SmtpOptions"/> (built-in System.Net.Mail, no extra package).</summary>
public sealed class SmtpEmailSender : IEmailSender
{
    private readonly SmtpOptions _options;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<SmtpOptions> options, ILogger<SmtpEmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public bool IsConfigured => _options.IsConfigured;

    public async Task<bool> SendAsync(string toAddress, string? toName, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        if (!_options.IsConfigured)
        {
            _logger.LogWarning("SMTP is not configured (section \"{Section}\"); email to {To} NOT sent. Subject: {Subject}\n{Body}",
                SmtpOptions.SectionName, toAddress, subject, htmlBody);
            return false;
        }

        // Empty strings in appsettings.json count as "not set".
        var from = !string.IsNullOrWhiteSpace(_options.FromAddress) ? _options.FromAddress
            : !string.IsNullOrWhiteSpace(_options.UserName) ? _options.UserName
            : throw new InvalidOperationException("Smtp:FromAddress (or Smtp:UserName) is required.");

        using var message = new MailMessage
        {
            From = new MailAddress(from, _options.FromName, Encoding.UTF8),
            Subject = subject,
            SubjectEncoding = Encoding.UTF8,
            Body = htmlBody,
            BodyEncoding = Encoding.UTF8,
            IsBodyHtml = true
        };
        message.To.Add(new MailAddress(toAddress, toName ?? string.Empty, Encoding.UTF8));

        using var client = new SmtpClient(_options.Host, _options.Port)
        {
            EnableSsl = _options.EnableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network
        };
        if (!string.IsNullOrEmpty(_options.UserName))
        {
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(_options.UserName, _options.Password);
        }

        await client.SendMailAsync(message, cancellationToken);
        _logger.LogInformation("Email \"{Subject}\" sent to {To}.", subject, toAddress);
        return true;
    }
}

public sealed class AccountEmailService : IAccountEmailService
{
    private readonly IEmailSender _emailSender;
    private readonly ILogger<AccountEmailService> _logger;

    public AccountEmailService(IEmailSender emailSender, ILogger<AccountEmailService> logger)
    {
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task<bool> SendEmailConfirmationAsync(string toAddress, string? fullName, string confirmationUrl, CancellationToken cancellationToken = default)
    {
        var name = WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(fullName) ? "bạn" : fullName);
        var link = WebUtility.HtmlEncode(confirmationUrl);
        var body = $"""
            <div style="font-family:Arial,Helvetica,sans-serif;font-size:15px;color:#222;max-width:560px">
              <h2 style="color:#e53637;margin-bottom:8px">PôpPôp – Xác thực email</h2>
              <p>Xin chào {name},</p>
              <p>Cảm ơn bạn đã đăng ký tài khoản tại cửa hàng cầu lông PôpPôp. Vui lòng bấm nút bên dưới để xác thực địa chỉ email và kích hoạt tài khoản:</p>
              <p style="margin:24px 0">
                <a href="{link}" style="background:#e53637;color:#fff;text-decoration:none;padding:12px 24px;border-radius:4px;font-weight:bold">Xác thực email</a>
              </p>
              <p>Nếu nút không hoạt động, hãy sao chép đường dẫn sau vào trình duyệt:<br /><a href="{link}">{link}</a></p>
              <p>Liên kết có hiệu lực trong 24 giờ. Nếu bạn không đăng ký tài khoản, hãy bỏ qua email này.</p>
            </div>
            """;

        try
        {
            return await _emailSender.SendAsync(toAddress, fullName, "Xác thực email tài khoản PôpPôp", body, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            // A mail problem must never break registration/login: the page tells the user it could not be sent.
            _logger.LogError(exception, "Could not send the confirmation email to {To}.", toAddress);
            return false;
        }
    }
}
