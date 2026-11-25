using System.Security.Cryptography;

namespace PopPop.Services.Implementations;

/// <summary>
/// Human-friendly reference codes such as PP-251123-4F2KQ7 / BH-251123-7XK2PM:
/// prefix + Vietnam-local date (yyMMdd) + 6 random characters.
/// The alphabet leaves out look-alikes (0/O, 1/I/L) so codes can be read out over the phone;
/// 31^6 ≈ 887 million combinations per day, drawn from a cryptographic RNG so codes cannot be guessed.
/// </summary>
public static class ShortCodeGenerator
{
    private const string Alphabet = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";
    private const int RandomLength = 6;
    private const int MaxAttempts = 5;

    public static string Create(string prefix, DateTime utcNow)
    {
        var date = ShopClock.ToLocal(utcNow).ToString("yyMMdd", System.Globalization.CultureInfo.InvariantCulture);
        Span<char> random = stackalloc char[RandomLength];
        for (var index = 0; index < RandomLength; index++)
        {
            random[index] = Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)];
        }

        return $"{prefix}-{date}-{new string(random)}";
    }

    /// <summary>Creates a code that <paramref name="existsAsync"/> reports as unused (collisions are astronomically rare).</summary>
    public static async Task<string> CreateUniqueAsync(
        string prefix,
        DateTime utcNow,
        Func<string, CancellationToken, Task<bool>> existsAsync,
        CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < MaxAttempts; attempt++)
        {
            var code = Create(prefix, utcNow);
            if (!await existsAsync(code, cancellationToken))
            {
                return code;
            }
        }

        throw new InvalidOperationException("Không tạo được mã tham chiếu. Vui lòng thử lại.");
    }
}
