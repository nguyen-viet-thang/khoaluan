using PopPop.Models;

namespace PopPop.Repositories.Interfaces;

/// <summary>Catalog reads for the racket advisor (on-sale products only).</summary>
public interface IAdvisorRepository
{
    /// <summary>Rackets: products of the category that owns the "Điểm cân bằng" specification.</summary>
    Task<IReadOnlyList<RacketCandidate>> GetRacketsAsync(CancellationToken cancellationToken = default);

    /// <summary>Strings with a gauge ("Đường kính") and an in-stock variant (cheapest one).</summary>
    Task<IReadOnlyList<StringCandidate>> GetStringsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<HeaderMenuLink>> GetRacketBrandsAsync(CancellationToken cancellationToken = default);
}
