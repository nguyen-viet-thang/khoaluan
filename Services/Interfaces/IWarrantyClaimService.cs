using PopPop.Data.Entities.Enums;
using PopPop.Models;

namespace PopPop.Services.Interfaces;

/// <summary>
/// Warranty / receipt-defect claims with video evidence.
/// Rules: only delivered (completed) orders; receipt defects within the brand's report window;
/// warranty within the brand × category warranty period; excluded defects are refused;
/// at least one video is required; one open claim per order line.
/// No refunds: a claim ends with a new product (shop exchange / brand replacement) or a rejection.
/// Business-rule violations throw <see cref="InvalidOperationException"/>; unknown items throw <see cref="KeyNotFoundException"/>.
/// </summary>
public interface IWarrantyClaimService
{
    ClaimMediaLimits Limits { get; }

    // ----- Customer -----
    /// <summary>Step 1: the customer's delivered orders with each product and whether it can be claimed now.</summary>
    Task<ClaimStartViewModel> GetStartAsync(string userId, string? search, CancellationToken cancellationToken = default);
    /// <summary>
    /// Step 2 form. With <paramref name="orderItemId"/> (picked on step 1) only that product is shown;
    /// without it (or when it cannot be claimed) the customer picks from the order's lines.
    /// </summary>
    Task<CreateClaimViewModel?> GetCreateFormAsync(int orderId, int? orderItemId, string userId, CancellationToken cancellationToken = default);
    /// <summary>Reloads items/defects after a failed post.</summary>
    Task PopulateCreateFormAsync(CreateClaimViewModel model, string userId, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(string userId, string actorName, CreateClaimViewModel model, IReadOnlyList<IFormFile> files, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ClaimRowViewModel>> GetForCustomerAsync(string userId, int? orderId = null, CancellationToken cancellationToken = default);
    Task<ClaimDetailsViewModel?> GetDetailsForCustomerAsync(int claimId, string userId, CancellationToken cancellationToken = default);
    Task AddEvidenceAsync(int claimId, string userId, string actorName, string? note, IReadOnlyList<IFormFile> files, CancellationToken cancellationToken = default);
    Task CancelAsync(int claimId, string userId, string actorName, CancellationToken cancellationToken = default);
    /// <summary>After approval: the customer tells the shop how the item was sent in (can be corrected until the shop receives it).</summary>
    Task ReportShipmentAsync(int claimId, string userId, string actorName, ClaimShipmentInput input, CancellationToken cancellationToken = default);
    /// <summary>The customer received the replacement → claim completed.</summary>
    Task ConfirmReceivedAsync(int claimId, string userId, string actorName, CancellationToken cancellationToken = default);

    // ----- Staff / admin -----
    Task<ClaimListViewModel> GetForStaffAsync(ClaimStatus? status, ClaimType? type, string? search, CancellationToken cancellationToken = default);
    Task<ClaimDetailsViewModel?> GetDetailsForStaffAsync(int claimId, CancellationToken cancellationToken = default);
    Task DecideAsync(int claimId, string actorUserId, string actorName, ClaimDecisionViewModel decision, CancellationToken cancellationToken = default);
    Task SaveStaffNoteAsync(int claimId, string? note, CancellationToken cancellationToken = default);
    /// <summary>Message to the customer without changing the status (e.g. ask for refund account, give the shop address).</summary>
    Task SendMessageAsync(int claimId, string actorUserId, string actorName, string message, CancellationToken cancellationToken = default);

    /// <summary>Evidence file for playback. Customers only get files of their own claims (userId != null); staff get any.</summary>
    Task<(string Path, string ContentType, string FileName)?> GetMediaFileAsync(int mediaId, string? userId, CancellationToken cancellationToken = default);
}
