namespace PopPop.Models;

/// <summary>
/// Outgoing mail server, bound from the "Smtp" section (put the password in user-secrets, not appsettings.json).
/// When <see cref="Host"/> is empty no mail is sent: the message (with its link) is written to the log instead.
/// </summary>
public sealed class SmtpOptions
{
    public const string SectionName = "Smtp";

    public string? Host { get; set; }
    public int Port { get; set; } = 587;
    /// <summary>STARTTLS (port 587) / SSL. Gmail, Outlook and most providers require it.</summary>
    public bool EnableSsl { get; set; } = true;
    public string? UserName { get; set; }
    public string? Password { get; set; }
    /// <summary>Sender address; defaults to <see cref="UserName"/>.</summary>
    public string? FromAddress { get; set; }
    public string FromName { get; set; } = "PôpPôp";

    public bool IsConfigured => !string.IsNullOrWhiteSpace(Host);
}
