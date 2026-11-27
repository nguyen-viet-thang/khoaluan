using PopPop.Models;

namespace PopPop.Services.Interfaces;

/// <summary>
/// Turns a player's profile (level, play style, wrist strength, budget) into racket specifications and ranks the
/// rackets in stock by an explainable fit score, so buyers do not need to understand 3U/4U, balance or flex.
/// </summary>
public interface IRacketAdvisorService
{
    Task<AdvisorResult> RecommendAsync(PlayerProfile profile, CancellationToken cancellationToken = default);

    /// <summary>Fit of one racket for the player, or null when the product is not a racket.</summary>
    Task<RacketMatch?> EvaluateAsync(PlayerProfile profile, int productId, CancellationToken cancellationToken = default);

    /// <summary>True when the product is a racket the advisor can score (used for the call to action).</summary>
    Task<bool> IsRacketAsync(int productId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<HeaderMenuLink>> GetBrandsAsync(CancellationToken cancellationToken = default);
}
