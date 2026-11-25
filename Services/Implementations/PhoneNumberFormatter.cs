namespace PopPop.Services.Implementations;

/// <summary>
/// Vietnamese phone numbers. The normalised form (0xxxxxxxxx) is also the account user name used to sign in.
/// </summary>
public static class PhoneNumberFormatter
{
    /// <summary>Removes spaces, dots and dashes and turns a +84 / 84 prefix into 0.</summary>
    public static string? Normalize(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return null;
        }

        var compact = string.Concat(phone.Where(character => !char.IsWhiteSpace(character) && character is not '.' and not '-'));
        if (compact.StartsWith("+84", StringComparison.Ordinal))
        {
            return "0" + compact[3..];
        }

        return compact.StartsWith("84", StringComparison.Ordinal) && compact.Length == 11 ? "0" + compact[2..] : compact;
    }

    /// <summary>A 10-digit Vietnamese number starting with 0 (after normalisation).</summary>
    public static bool IsValid(string? normalized) =>
        normalized is { Length: 10 } && normalized[0] == '0' && normalized.All(char.IsAsciiDigit);

    public const string InvalidMessage = "Số điện thoại phải gồm 10 chữ số và bắt đầu bằng 0 (VD: 0901234567).";
}
