using System.Globalization;

namespace PopPop.Models;

public enum PlayerLevel { Beginner = 1, Intermediate = 2, Advanced = 3 }

public enum PlayStyle { Attack = 1, Defense = 2, AllRound = 3, Net = 4 }

public enum WristStrength { Normal = 1, Strong = 2 }

public enum RacketBudget { Under1M = 1, To2M = 2, To3_5M = 3, Any = 4 }

/// <summary>
/// The customer's answers to the racket advisor (bound from the query string and kept in a cookie,
/// so racket pages can show "how well this fits you").
/// </summary>
public sealed class PlayerProfile
{
    public PlayerLevel? Level { get; set; }
    public PlayStyle? Style { get; set; }
    public WristStrength? Wrist { get; set; }
    public RacketBudget? Budget { get; set; }
    /// <summary>Preferred brand (optional).</summary>
    public int? BrandId { get; set; }

    public bool IsComplete => Level.HasValue && Style.HasValue && Wrist.HasValue && Budget.HasValue;

    public decimal? MaxPrice => Budget switch
    {
        RacketBudget.Under1M => 1_000_000m,
        RacketBudget.To2M => 2_000_000m,
        RacketBudget.To3_5M => 3_500_000m,
        _ => null
    };

    /// <summary>Query-string form, also the cookie value: "level=Beginner&amp;style=Attack&amp;...".</summary>
    public string ToQueryString()
    {
        var parts = new List<string>
        {
            $"level={Level}", $"style={Style}", $"wrist={Wrist}", $"budget={Budget}"
        };
        if (BrandId.HasValue)
        {
            parts.Add($"brandId={BrandId.Value.ToString(CultureInfo.InvariantCulture)}");
        }

        return string.Join("&", parts);
    }

    public static PlayerProfile? Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var profile = new PlayerProfile();
        foreach (var pair in value.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var (key, text) = pair.Split('=', 2) switch
            {
                [var k, var v] => (k.ToLowerInvariant(), v),
                _ => (string.Empty, string.Empty)
            };
            switch (key)
            {
                case "level" when Enum.TryParse<PlayerLevel>(text, out var level): profile.Level = level; break;
                case "style" when Enum.TryParse<PlayStyle>(text, out var style): profile.Style = style; break;
                case "wrist" when Enum.TryParse<WristStrength>(text, out var wrist): profile.Wrist = wrist; break;
                case "budget" when Enum.TryParse<RacketBudget>(text, out var budget): profile.Budget = budget; break;
                case "brandid" when int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out var brandId): profile.BrandId = brandId; break;
            }
        }

        return profile.IsComplete ? profile : null;
    }
}

public enum FitTone { Good = 1, Neutral = 2, Warning = 3 }

public sealed record FitReason(FitTone Tone, string Text);

/// <summary>One recommended racket with its explainable fit score.</summary>
public sealed class RacketMatch
{
    public int ProductId { get; init; }
    public required string Name { get; init; }
    public string? BrandName { get; init; }
    public string? ImageUrl { get; init; }
    public int MatchPercent { get; init; }
    /// <summary>The variant (weight/grip) that suits the player best and is in stock.</summary>
    public int? VariantId { get; init; }
    public string? VariantLabel { get; init; }
    public decimal Price { get; init; }
    public string? Balance { get; init; }
    public string? Stiffness { get; init; }
    public bool OverBudget { get; init; }
    public IReadOnlyList<FitReason> Reasons { get; init; } = [];
}

/// <summary>String (cước) to string the recommended racket with.</summary>
public sealed class StringSuggestion
{
    public int ProductId { get; init; }
    public required string Name { get; init; }
    public string? ImageUrl { get; init; }
    public int VariantId { get; init; }
    public decimal Price { get; init; }
    public required string Gauge { get; init; }
    public required string Why { get; init; }
}

public sealed class AdvisorResult
{
    public required PlayerProfile Profile { get; init; }
    /// <summary>The ideal racket in plain specs, e.g. "Nhẹ đầu · Thân dẻo · 4U (80–84 g)".</summary>
    public IReadOnlyList<string> IdealSpecs { get; init; } = [];
    public IReadOnlyList<RacketMatch> Matches { get; init; } = [];
    /// <summary>True when nothing fits the budget, so the matches ignore it.</summary>
    public bool BudgetRelaxed { get; init; }
    public StringSuggestion? String { get; init; }
}

public sealed class AdvisorPageViewModel
{
    public PlayerProfile Profile { get; init; } = new();
    public AdvisorResult? Result { get; init; }
    public IReadOnlyList<HeaderMenuLink> Brands { get; init; } = [];
}

/// <summary>Racket advisor data read from the catalog (specs resolved to their option values).</summary>
public sealed record RacketCandidate(
    int ProductId,
    string Name,
    int BrandId,
    string BrandName,
    string? ImageUrl,
    SpecChoice? Balance,
    SpecChoice? Stiffness,
    IReadOnlyList<RacketVariantCandidate> Variants);

public sealed record RacketVariantCandidate(int VariantId, string? Label, decimal Price, int Stock, SpecChoice? Weight);

public sealed record SpecChoice(string Value, string? Hint);

public sealed record StringCandidate(int ProductId, string Name, string? ImageUrl, int VariantId, decimal Price, string Gauge);

/// <summary>Racket page panel: the fit for the saved profile, or (no profile yet) whether to invite the customer to the advisor.</summary>
public sealed record RacketFitPanel(RacketMatch? Fit, bool IsRacket);
