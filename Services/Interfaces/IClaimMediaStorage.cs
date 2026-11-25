using PopPop.Data.Entities.Enums;
using PopPop.Models;

namespace PopPop.Services.Interfaces;

/// <summary>
/// Private storage for claim evidence (videos/photos). Files live outside wwwroot and are only
/// served through authorised controller actions, because they may show customers' homes or faces.
/// </summary>
public interface IClaimMediaStorage
{
    ClaimMediaLimits Limits { get; }

    /// <summary>Checks extension, size and the real file signature. Returns the media kind or a user-facing error.</summary>
    Task<(ClaimMediaKind? Kind, string? Error)> ValidateAsync(IFormFile file, CancellationToken cancellationToken = default);

    /// <summary>Saves the file under the claim's folder and returns the random stored file name.</summary>
    Task<string> SaveAsync(int claimId, IFormFile file, CancellationToken cancellationToken = default);

    /// <summary>Absolute path of a stored file, or null when it does not exist.</summary>
    string? GetPath(int claimId, string storedFileName);

    void Delete(int claimId, string storedFileName);
}
