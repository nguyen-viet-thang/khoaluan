using System.Globalization;
using Microsoft.Extensions.Caching.Memory;
using PopPop.Models;
using PopPop.Repositories.Interfaces;
using PopPop.Services.Interfaces;

namespace PopPop.Services.Implementations;

/// <summary>
/// Rule-based, explainable racket recommendation.
/// Fit = 35% balance (play style) + 30% shaft flex (level, wrist) + 15% weight (level, style) + 20% price (budget),
/// plus a small bonus for the preferred brand. Every part produces a sentence, so the customer sees WHY a racket fits.
/// The rules follow the usual badminton advice: head-heavy for smashing, head-light for defence and net play,
/// flexible shafts and lighter frames for beginners, stiff shafts only for players with a strong wrist.
/// </summary>
public sealed class RacketAdvisorService(IAdvisorRepository repository, IMemoryCache cache) : IRacketAdvisorService
{
    private const int TopMatches = 3;
    private const decimal BudgetTolerance = 1.15m;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(2);

    private const string HeadHeavy = "Nặng đầu", Even = "Cân bằng", HeadLight = "Nhẹ đầu";
    private const string Flexible = "Dẻo", Medium = "Trung bình", Stiff = "Cứng";

    public async Task<AdvisorResult> RecommendAsync(PlayerProfile profile, CancellationToken cancellationToken = default)
    {
        var rackets = await GetRacketsAsync(cancellationToken);
        var scored = rackets.Select(racket => Score(profile, racket, enforceBudget: true)).OfType<RacketMatch>().ToList();
        var relaxed = false;
        if (scored.Count == 0)
        {
            // Nothing within budget: show the closest fits anyway, clearly marked as over budget.
            relaxed = true;
            scored = rackets.Select(racket => Score(profile, racket, enforceBudget: false)).OfType<RacketMatch>().ToList();
        }

        var matches = scored
            .OrderByDescending(match => match.MatchPercent)
            .ThenBy(match => match.Price)
            .Take(TopMatches)
            .ToList();

        return new AdvisorResult
        {
            Profile = profile,
            IdealSpecs = DescribeIdeal(profile),
            Matches = matches,
            BudgetRelaxed = relaxed,
            String = await SuggestStringAsync(profile, cancellationToken)
        };
    }

    public async Task<RacketMatch?> EvaluateAsync(PlayerProfile profile, int productId, CancellationToken cancellationToken = default)
    {
        var racket = (await GetRacketsAsync(cancellationToken)).FirstOrDefault(item => item.ProductId == productId);
        return racket is null ? null : Score(profile, racket, enforceBudget: false);
    }

    public async Task<bool> IsRacketAsync(int productId, CancellationToken cancellationToken = default) =>
        (await GetRacketsAsync(cancellationToken)).Any(item => item.ProductId == productId);

    public Task<IReadOnlyList<HeaderMenuLink>> GetBrandsAsync(CancellationToken cancellationToken = default) =>
        cache.GetOrCreateAsync("advisor-brands", entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            return repository.GetRacketBrandsAsync(cancellationToken);
        })!;

    private async Task<IReadOnlyList<RacketCandidate>> GetRacketsAsync(CancellationToken cancellationToken) =>
        (await cache.GetOrCreateAsync("advisor-rackets", entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            return repository.GetRacketsAsync(cancellationToken);
        }))!;

    // ---------------------------------------------------------------- scoring

    private static RacketMatch? Score(PlayerProfile profile, RacketCandidate racket, bool enforceBudget)
    {
        var inStock = racket.Variants.Where(variant => variant.Stock > 0).ToList();
        if (inStock.Count == 0)
        {
            return null;
        }

        var maxPrice = profile.MaxPrice;
        var affordable = maxPrice is null ? inStock : inStock.Where(variant => variant.Price <= maxPrice.Value * BudgetTolerance).ToList();
        if (enforceBudget && affordable.Count == 0)
        {
            return null;
        }

        // Best variant: the weight that suits the player, then the cheapest.
        var variant = (affordable.Count > 0 ? affordable : inStock)
            .OrderByDescending(item => WeightScore(profile, item.Weight?.Value))
            .ThenBy(item => item.Price)
            .First();

        var reasons = new List<FitReason>();
        var balance = BalanceScore(profile.Style!.Value, racket.Balance?.Value);
        var flex = FlexScore(profile, racket.Stiffness?.Value);
        var weight = WeightScore(profile, variant.Weight?.Value);
        var price = PriceScore(maxPrice, variant.Price);

        reasons.Add(BalanceReason(profile.Style!.Value, racket.Balance, balance));
        reasons.Add(FlexReason(profile, racket.Stiffness, flex));
        if (variant.Weight is not null)
        {
            reasons.Add(WeightReason(variant.Weight, weight));
        }

        reasons.Add(PriceReason(maxPrice, variant.Price));

        var total = .35 * balance + .30 * flex + .15 * weight + .20 * price;
        if (profile.BrandId == racket.BrandId)
        {
            total += .05;
            reasons.Add(new FitReason(FitTone.Good, $"Đúng thương hiệu {racket.BrandName} bạn yêu thích."));
        }

        return new RacketMatch
        {
            ProductId = racket.ProductId,
            Name = racket.Name,
            BrandName = racket.BrandName,
            ImageUrl = racket.ImageUrl,
            MatchPercent = (int)Math.Round(Math.Clamp(total, 0, .99) * 100),
            VariantId = variant.VariantId,
            VariantLabel = variant.Label,
            Price = variant.Price,
            Balance = racket.Balance?.Value,
            Stiffness = racket.Stiffness?.Value,
            OverBudget = maxPrice.HasValue && variant.Price > maxPrice.Value,
            Reasons = reasons.OrderBy(reason => reason.Tone).ToList()
        };
    }

    private static double BalanceScore(PlayStyle style, string? balance) => (style, balance) switch
    {
        (_, null) => .5,
        (PlayStyle.Attack, HeadHeavy) => 1, (PlayStyle.Attack, Even) => .55, (PlayStyle.Attack, _) => .15,
        (PlayStyle.Defense, HeadLight) => 1, (PlayStyle.Defense, Even) => .6, (PlayStyle.Defense, _) => .15,
        (PlayStyle.Net, HeadLight) => 1, (PlayStyle.Net, Even) => .65, (PlayStyle.Net, _) => .2,
        (PlayStyle.AllRound, Even) => 1, (PlayStyle.AllRound, _) => .6,
        _ => .5
    };

    private static double FlexScore(PlayerProfile profile, string? stiffness)
    {
        var strong = profile.Wrist == WristStrength.Strong;
        return (profile.Level, stiffness) switch
        {
            (_, null) => .5,
            (PlayerLevel.Beginner, Flexible) => strong ? .9 : 1,
            (PlayerLevel.Beginner, Medium) => strong ? .85 : .6,
            (PlayerLevel.Beginner, _) => strong ? .35 : .1,
            (PlayerLevel.Intermediate, Medium) => 1,
            (PlayerLevel.Intermediate, Flexible) => strong ? .5 : .75,
            (PlayerLevel.Intermediate, _) => strong ? .85 : .4,
            (PlayerLevel.Advanced, Stiff) => strong ? 1 : .75,
            (PlayerLevel.Advanced, Medium) => strong ? .7 : .95,
            (PlayerLevel.Advanced, _) => .3,
            _ => .5
        };
    }

    private static double WeightScore(PlayerProfile profile, string? weight)
    {
        if (weight is null)
        {
            return .5;
        }

        var light = profile.Level == PlayerLevel.Beginner || profile.Style is PlayStyle.Defense or PlayStyle.Net;
        var power = !light && profile.Style == PlayStyle.Attack && profile.Wrist == WristStrength.Strong;
        return (light, power, weight) switch
        {
            (true, _, "4U") => 1,
            (true, _, "8U") => profile.Level == PlayerLevel.Beginner ? .9 : .6,
            (true, _, _) => .5,
            (_, true, "3U") => 1,
            (_, true, "4U") => .75,
            (_, true, _) => .3,
            (_, _, "4U") => 1,
            (_, _, "3U") => .85,
            _ => .5
        };
    }

    private static double PriceScore(decimal? maxPrice, decimal price) =>
        maxPrice is null || price <= maxPrice.Value ? 1
        : price <= maxPrice.Value * BudgetTolerance ? .5
        : .1;

    // ---------------------------------------------------------------- explanations

    private static string StyleText(PlayStyle style) => style switch
    {
        PlayStyle.Attack => "lối đánh tấn công",
        PlayStyle.Defense => "lối chơi phòng thủ, phản tạt",
        PlayStyle.Net => "lối đánh đôi trên lưới",
        _ => "lối chơi công thủ toàn diện"
    };

    private static FitReason BalanceReason(PlayStyle style, SpecChoice? balance, double score)
    {
        if (balance is null)
        {
            return new FitReason(FitTone.Neutral, "Chưa có thông tin điểm cân bằng.");
        }

        var detail = balance.Hint is null ? string.Empty : $" ({balance.Hint.ToLowerInvariant()})";
        return score >= .9
            ? new FitReason(FitTone.Good, $"{balance.Value}{detail}: đúng {StyleText(style)} của bạn.")
            : score >= .5
                ? new FitReason(FitTone.Neutral, $"{balance.Value}{detail}: vẫn dùng tốt cho {StyleText(style)}.")
                : new FitReason(FitTone.Warning, $"{balance.Value}{detail}: không thật hợp {StyleText(style)}.");
    }

    private static FitReason FlexReason(PlayerProfile profile, SpecChoice? stiffness, double score)
    {
        if (stiffness is null)
        {
            return new FitReason(FitTone.Neutral, "Chưa có thông tin độ cứng thân.");
        }

        var text = $"Thân {stiffness.Value.ToLowerInvariant()}";
        return score >= .85
            ? new FitReason(FitTone.Good, stiffness.Value == Flexible
                ? $"{text}: vợt tự trợ lực, đánh cầu đi xa mà không cần dùng nhiều sức."
                : stiffness.Value == Stiff
                    ? $"{text}: kiểm soát chính xác, tận dụng tốt lực cổ tay của bạn."
                    : $"{text}: cân đối giữa trợ lực và kiểm soát, hợp trình độ của bạn.")
            : score >= .5
                ? new FitReason(FitTone.Neutral, $"{text}: phù hợp ở mức chấp nhận được với trình độ của bạn.")
                : new FitReason(FitTone.Warning, stiffness.Value == Stiff
                    ? $"{text}: cần lực cổ tay tốt, dễ mỏi tay nếu chưa quen."
                    : $"{text}: hơi mềm so với lực đánh của bạn, cầu đi kém chính xác.");
    }

    private static FitReason WeightReason(SpecChoice weight, double score)
    {
        var grams = weight.Hint is null ? string.Empty : $" ({weight.Hint})";
        return score >= .9
            ? new FitReason(FitTone.Good, $"Bản {weight.Value}{grams} là trọng lượng hợp nhất với bạn.")
            : new FitReason(FitTone.Neutral, $"Bản {weight.Value}{grams} còn hàng gần nhất với trọng lượng bạn cần.");
    }

    private static FitReason PriceReason(decimal? maxPrice, decimal price)
    {
        if (maxPrice is null || price <= maxPrice.Value)
        {
            return new FitReason(FitTone.Good, maxPrice is null ? "Giá phù hợp lựa chọn không giới hạn ngân sách." : "Nằm trong ngân sách của bạn.");
        }

        var over = (int)Math.Ceiling((price - maxPrice.Value) / maxPrice.Value * 100);
        return new FitReason(FitTone.Warning, $"Vượt ngân sách khoảng {over}% ({price - maxPrice.Value:N0} ₫).");
    }

    private static IReadOnlyList<string> DescribeIdeal(PlayerProfile profile)
    {
        var balance = profile.Style switch
        {
            PlayStyle.Attack => "Nặng đầu",
            PlayStyle.Defense or PlayStyle.Net => "Nhẹ đầu",
            _ => "Cân bằng"
        };
        var flex = (profile.Level, profile.Wrist) switch
        {
            (PlayerLevel.Beginner, _) => "Thân dẻo",
            (PlayerLevel.Advanced, WristStrength.Strong) => "Thân cứng",
            (PlayerLevel.Intermediate, WristStrength.Strong) => "Thân trung bình – cứng",
            _ => "Thân trung bình"
        };
        var light = profile.Level == PlayerLevel.Beginner || profile.Style is PlayStyle.Defense or PlayStyle.Net;
        var weight = !light && profile.Style == PlayStyle.Attack && profile.Wrist == WristStrength.Strong ? "3U (85–89 g)" : "4U (80–84 g)";
        var budget = profile.MaxPrice is { } max ? $"Dưới {max / 1_000_000m:0.#} triệu" : "Không giới hạn giá";
        return [balance, flex, weight, budget];
    }

    /// <summary>
    /// Gauge rule of thumb: beginners wear strings out fast → thick and durable (0.68–0.70 mm);
    /// advanced or attacking players want repulsion and feel → thin (0.63–0.66 mm).
    /// </summary>
    private async Task<StringSuggestion?> SuggestStringAsync(PlayerProfile profile, CancellationToken cancellationToken)
    {
        var strings = await cache.GetOrCreateAsync("advisor-strings", entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            return repository.GetStringsAsync(cancellationToken);
        });
        if (strings is null || strings.Count == 0)
        {
            return null;
        }

        var (target, why) = profile.Level switch
        {
            PlayerLevel.Beginner => (0.70m, "Cước dày, bền: hợp người mới vì ít bị đứt khi đánh chưa trúng tâm vợt."),
            PlayerLevel.Advanced when profile.Style == PlayStyle.Attack => (0.65m, "Cước mảnh: độ nảy cao, tiếng đanh, tăng uy lực cú đập."),
            PlayerLevel.Advanced => (0.66m, "Cước mảnh vừa: nảy tốt mà vẫn đủ bền cho người chơi nhiều."),
            _ when profile.Style == PlayStyle.Attack => (0.66m, "Cước mảnh vừa: tăng độ nảy cho cú đập mà vẫn đủ bền."),
            _ => (0.68m, "Cước trung bình: cân bằng giữa độ nảy và độ bền.")
        };

        var best = strings
            .Select(item => (item, gauge: ParseGauge(item.Gauge)))
            .Where(item => item.gauge.HasValue)
            .OrderBy(item => Math.Abs(item.gauge!.Value - target))
            .ThenBy(item => item.item.Price)
            .Select(item => item.item)
            .FirstOrDefault();

        return best is null
            ? null
            : new StringSuggestion
            {
                ProductId = best.ProductId,
                Name = best.Name,
                ImageUrl = best.ImageUrl,
                VariantId = best.VariantId,
                Price = best.Price,
                Gauge = best.Gauge,
                Why = why
            };
    }

    /// <summary>"0,65 mm" → 0.65.</summary>
    private static decimal? ParseGauge(string value)
    {
        var number = value.Replace("mm", string.Empty, StringComparison.OrdinalIgnoreCase).Trim().Replace(',', '.');
        return decimal.TryParse(number, NumberStyles.Number, CultureInfo.InvariantCulture, out var gauge) ? gauge : null;
    }
}
