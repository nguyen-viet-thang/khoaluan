namespace PopPop.Services.Interfaces;

public interface IEmailSender
{
    /// <summary>True when a mail server is configured (otherwise messages only go to the log).</summary>
    bool IsConfigured { get; }

    /// <summary>
    /// Sends an HTML email. Returns false when no mail server is configured (the message is logged instead).
    /// Throws when the server rejects the message.
    /// </summary>
    Task<bool> SendAsync(string toAddress, string? toName, string subject, string htmlBody, CancellationToken cancellationToken = default);
}

/// <summary>Account emails (address verification).</summary>
public interface IAccountEmailService
{
    /// <summary>
    /// Emails the verification link to <paramref name="toAddress"/>. Returns whether it was really delivered to a
    /// mail server (false = no SMTP configured or sending failed; the reason is logged, never thrown).
    /// </summary>
    Task<bool> SendEmailConfirmationAsync(string toAddress, string? fullName, string confirmationUrl, CancellationToken cancellationToken = default);

    /// <summary>Emails the "set a new password" link. Same delivery contract as <see cref="SendEmailConfirmationAsync"/>.</summary>
    Task<bool> SendPasswordResetAsync(string toAddress, string? fullName, string resetUrl, CancellationToken cancellationToken = default);
}
