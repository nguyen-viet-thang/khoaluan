using Microsoft.Extensions.Options;
using PopPop.Data.Entities;
using PopPop.Data.Entities.Enums;
using PopPop.Models;
using PopPop.Repositories.Interfaces;
using PopPop.Services.Interfaces;

namespace PopPop.Services.Implementations;

public sealed class WarrantyClaimService : IWarrantyClaimService
{
    private readonly IWarrantyClaimRepository _claimRepository;
    private readonly IClaimMediaStorage _mediaStorage;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly StoreOptions _store;

    public WarrantyClaimService(
        IWarrantyClaimRepository claimRepository,
        IClaimMediaStorage mediaStorage,
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        IOptions<StoreOptions> storeOptions)
    {
        _claimRepository = claimRepository;
        _mediaStorage = mediaStorage;
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _store = storeOptions.Value;
    }

    public ClaimMediaLimits Limits => _mediaStorage.Limits;

    #region Customer

    public async Task<ClaimStartViewModel> GetStartAsync(string userId, string? search, CancellationToken cancellationToken = default)
    {
        var orders = await _claimRepository.GetDeliveredOrdersForClaimAsync(userId, search, cancellationToken);
        var lines = orders.SelectMany(order => order.OrderItems).Where(item => item.ProductVariant?.Product is not null).ToList();
        var brandIds = lines.Select(item => item.ProductVariant!.Product!.BrandId).Distinct().ToList();
        var policies = await _claimRepository.GetPoliciesAsync(brandIds, cancellationToken);
        var openClaims = await _claimRepository.GetOpenClaimCodesAsync(lines.Select(item => item.Id).ToList(), cancellationToken);
        var now = DateTime.UtcNow;

        return new ClaimStartViewModel
        {
            Search = search?.Trim(),
            UndeliveredOrderCount = await _claimRepository.CountUndeliveredOrdersAsync(userId, cancellationToken),
            Orders = orders.Select(order => new ClaimOrderOptionViewModel
            {
                OrderId = order.Id,
                OrderCode = order.OrderCode,
                CreatedAt = order.CreatedAt,
                DeliveredAt = DeliveredAt(order),
                TotalAmount = order.TotalAmount,
                RecipientName = order.RecipientName,
                RecipientPhone = order.RecipientPhone,
                ShippingAddress = order.ShippingAddress,
                Items = BuildItemOptions(order, policies, openClaims, now)
            }).ToList()
        };
    }

    public async Task<CreateClaimViewModel?> GetCreateFormAsync(int orderId, int? orderItemId, string userId, CancellationToken cancellationToken = default)
    {
        var order = await _claimRepository.GetOrderForClaimAsync(orderId, userId, cancellationToken);
        if (order is null)
        {
            return null;
        }

        EnsureDelivered(order);
        var model = new CreateClaimViewModel { OrderId = orderId };
        await FillOptionsAsync(model, order, cancellationToken);

        // Product picked on step 1: lock the form to it (only if it can still be claimed, else fall back to the list).
        if (orderItemId.HasValue && model.Items.Any(item => item.OrderItemId == orderItemId && item.CanClaim))
        {
            model.OrderItemId = orderItemId;
            model.ProductLocked = true;
        }

        return model;
    }

    public async Task PopulateCreateFormAsync(CreateClaimViewModel model, string userId, CancellationToken cancellationToken = default)
    {
        var order = await _claimRepository.GetOrderForClaimAsync(model.OrderId, userId, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy đơn hàng.");
        await FillOptionsAsync(model, order, cancellationToken);
    }

    public async Task<int> CreateAsync(
        string userId,
        string actorName,
        CreateClaimViewModel model,
        IReadOnlyList<IFormFile> files,
        CancellationToken cancellationToken = default)
    {
        var order = await _claimRepository.GetOrderForClaimAsync(model.OrderId, userId, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy đơn hàng.");
        EnsureDelivered(order);

        var item = order.OrderItems.FirstOrDefault(line => line.Id == model.OrderItemId)
            ?? throw new InvalidOperationException("Sản phẩm đã chọn không thuộc đơn hàng này.");
        var product = item.ProductVariant?.Product
            ?? throw new InvalidOperationException("Không tìm thấy thông tin sản phẩm.");
        var defect = await _claimRepository.GetDefectTypeAsync(model.DefectTypeId ?? 0, cancellationToken);
        if (defect is null || !defect.IsActive || defect.CategoryId != product.CategoryId)
        {
            throw new InvalidOperationException("Loại lỗi đã chọn không áp dụng cho sản phẩm này.");
        }

        if (defect.Coverage == DefectCoverage.NotCovered)
        {
            throw new InvalidOperationException(
                $"\"{defect.Name}\" không thuộc diện bảo hành hoặc đổi hàng (căn cứ: {defect.Source}).");
        }

        var policy = await _claimRepository.GetPolicyAsync(product.BrandId, product.CategoryId, cancellationToken);
        var window = ComputeWindow(order, product.Brand!, policy);
        var now = DateTime.UtcNow;
        var type = defect.Coverage == DefectCoverage.Warranty ? ClaimType.Warranty : ClaimType.ReceiptDefect;
        DateTime deadline;
        if (type == ClaimType.ReceiptDefect)
        {
            if (now > window.ReceiptDeadline)
            {
                throw new InvalidOperationException(
                    $"Đã quá thời hạn báo lỗi khi nhận hàng ({window.ReceiptDays} ngày kể từ ngày giao, hết hạn {ToLocalText(window.ReceiptDeadline)}).");
            }

            deadline = window.ReceiptDeadline;
        }
        else
        {
            if (window.WarrantyDeadline is null)
            {
                throw new InvalidOperationException($"Sản phẩm {product.Brand!.Name} thuộc danh mục này không có chính sách bảo hành của hãng.");
            }

            if (now > window.WarrantyDeadline)
            {
                throw new InvalidOperationException(
                    $"Sản phẩm đã hết thời hạn bảo hành ({window.WarrantyDays} ngày kể từ ngày giao, hết hạn {ToLocalText(window.WarrantyDeadline.Value)}).");
            }

            deadline = window.WarrantyDeadline.Value;
        }

        if (model.Quantity < 1 || model.Quantity > item.Quantity)
        {
            throw new InvalidOperationException($"Số lượng phải từ 1 đến {item.Quantity}.");
        }

        var openClaims = await _claimRepository.GetOpenClaimCodesAsync([item.Id], cancellationToken);
        if (openClaims.TryGetValue(item.Id, out var openCode))
        {
            throw new InvalidOperationException($"Sản phẩm này đang có yêu cầu {openCode} chưa xử lý xong.");
        }

        var media = await ValidateFilesAsync(files, requireVideo: true, cancellationToken);

        var savedFiles = new List<(int ClaimId, string Name)>();
        int claimId;
        var claimCode = string.Empty;
        try
        {
            claimId = await _unitOfWork.ExecuteInTransactionAsync(async token =>
            {
                var claim = new WarrantyClaim
                {
                    Code = await ShortCodeGenerator.CreateUniqueAsync("BH", now, _claimRepository.ClaimCodeExistsAsync, token),
                    UserId = userId,
                    OrderId = order.Id,
                    OrderItemId = item.Id,
                    DefectTypeId = defect.Id,
                    Type = type,
                    Status = ClaimStatus.Submitted,
                    Quantity = model.Quantity,
                    Description = model.Description.Trim(),
                    SerialNumber = string.IsNullOrWhiteSpace(model.SerialNumber) ? null : model.SerialNumber.Trim(),
                    DeadlineAt = deadline,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                claim.History.Add(NewHistory(null, ClaimStatus.Submitted, "Khách hàng gửi yêu cầu kèm minh chứng.", userId, actorName, byStaff: false, now));
                await _claimRepository.AddAsync(claim, token);
                await _unitOfWork.SaveChangesAsync(token); // assigns claim.Id for the media folder

                await AttachFilesAsync(claim, media, userId, byStaff: false, now, savedFiles, token);
                claimCode = claim.Code;
                return claim.Id;
            }, cancellationToken);
        }
        catch
        {
            foreach (var (savedClaimId, name) in savedFiles)
            {
                _mediaStorage.Delete(savedClaimId, name);
            }

            throw;
        }

        await _notificationService.NotifyStaffAsync(
            $"Yêu cầu {(type == ClaimType.Warranty ? "bảo hành" : "đổi hàng")} mới {claimCode}",
            $"{actorName} · {item.ProductName} · {defect.Code} – {defect.Name} · {media.Count} tệp minh chứng.",
            $"/Staff/Claims/Details/{claimId}",
            cancellationToken);
        return claimId;
    }

    public async Task<IReadOnlyList<ClaimRowViewModel>> GetForCustomerAsync(
        string userId,
        int? orderId = null,
        CancellationToken cancellationToken = default)
    {
        var claims = await _claimRepository.GetForUserAsync(userId, orderId, cancellationToken);
        return claims.Select(ToRow).ToList();
    }

    public async Task<ClaimDetailsViewModel?> GetDetailsForCustomerAsync(int claimId, string userId, CancellationToken cancellationToken = default)
    {
        var claim = await _claimRepository.GetDetailsAsync(claimId, cancellationToken);
        return claim is null || claim.UserId != userId ? null : ToDetails(claim, forStaff: false);
    }

    public async Task AddEvidenceAsync(
        int claimId,
        string userId,
        string actorName,
        string? note,
        IReadOnlyList<IFormFile> files,
        CancellationToken cancellationToken = default)
    {
        var claim = await _claimRepository.GetDetailsAsync(claimId, cancellationToken);
        if (claim is null || claim.UserId != userId)
        {
            throw new KeyNotFoundException("Không tìm thấy yêu cầu.");
        }

        if (claim.Status != ClaimStatus.NeedMoreInfo)
        {
            throw new InvalidOperationException("Chỉ bổ sung minh chứng khi cửa hàng yêu cầu.");
        }

        if (files.Count == 0)
        {
            throw new InvalidOperationException("Vui lòng chọn ít nhất một video hoặc ảnh bổ sung.");
        }

        var media = await ValidateFilesAsync(files, requireVideo: false, cancellationToken);
        var now = DateTime.UtcNow;
        var savedFiles = new List<(int ClaimId, string Name)>();
        try
        {
            await _unitOfWork.ExecuteInTransactionAsync(async token =>
            {
                await AttachFilesAsync(claim, media, userId, byStaff: false, now, savedFiles, token);
                ChangeStatus(claim, ClaimStatus.Submitted,
                    string.IsNullOrWhiteSpace(note) ? "Khách hàng đã bổ sung minh chứng." : $"Khách hàng bổ sung minh chứng: {note.Trim()}",
                    userId, actorName, byStaff: false, now);
                return true;
            }, cancellationToken);
        }
        catch
        {
            foreach (var (id, name) in savedFiles)
            {
                _mediaStorage.Delete(id, name);
            }

            throw;
        }

        await _notificationService.NotifyStaffAsync(
            $"Khách đã bổ sung minh chứng cho {claim.Code}",
            $"{actorName} gửi thêm {media.Count} tệp" + (string.IsNullOrWhiteSpace(note) ? "." : $": \"{note.Trim().TrimEnd('.')}\".") + " Yêu cầu đã quay lại hàng chờ duyệt.",
            $"/Staff/Claims/Details/{claim.Id}",
            cancellationToken);
    }

    public async Task CancelAsync(int claimId, string userId, string actorName, CancellationToken cancellationToken = default)
    {
        var claim = await _claimRepository.GetDetailsAsync(claimId, cancellationToken);
        if (claim is null || claim.UserId != userId)
        {
            throw new KeyNotFoundException("Không tìm thấy yêu cầu.");
        }

        if (!CustomerCanCancel(claim))
        {
            throw new InvalidOperationException(claim.ReturnShippedAt.HasValue
                ? "Bạn đã gửi sản phẩm về cửa hàng nên không thể tự hủy. Vui lòng liên hệ cửa hàng."
                : "Yêu cầu đang được cửa hàng xử lý nên không thể tự hủy. Vui lòng liên hệ cửa hàng.");
        }

        ChangeStatus(claim, ClaimStatus.Cancelled, "Khách hàng hủy yêu cầu.", userId, actorName, byStaff: false, DateTime.UtcNow);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _notificationService.NotifyStaffAsync(
            $"Khách đã hủy yêu cầu {claim.Code}",
            $"{actorName} tự hủy yêu cầu cho sản phẩm {claim.OrderItem?.ProductName}. Không cần xử lý thêm.",
            $"/Staff/Claims/Details/{claim.Id}",
            cancellationToken);
    }

    public async Task ReportShipmentAsync(
        int claimId,
        string userId,
        string actorName,
        ClaimShipmentInput input,
        CancellationToken cancellationToken = default)
    {
        var claim = await GetOwnClaimAsync(claimId, userId, cancellationToken);
        if (claim.Status != ClaimStatus.Approved)
        {
            throw new InvalidOperationException("Chỉ cập nhật thông tin gửi hàng khi yêu cầu đã được duyệt và cửa hàng chưa nhận hàng.");
        }

        var (carrier, tracking) = NormalizeShipment(input.Carrier, input.TrackingCode);
        var isCorrection = claim.ReturnShippedAt.HasValue;
        claim.ReturnCarrier = carrier;
        claim.ReturnTrackingCode = tracking;
        claim.ReturnShippedAt = DateTime.UtcNow;
        var text = ClaimDisplay.IsInPerson(carrier)
            ? "Khách sẽ mang sản phẩm trực tiếp đến cửa hàng."
            : $"Khách {(isCorrection ? "cập nhật thông tin" : "đã")} gửi sản phẩm {ClaimDisplay.ShipmentText(carrier, tracking)}.";
        AddUpdate(claim, text, userId, actorName, byStaff: false, claim.ReturnShippedAt.Value);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _notificationService.NotifyStaffAsync(
            $"Khách đã gửi hàng về cho yêu cầu {claim.Code}",
            $"{actorName} · {claim.OrderItem?.ProductName}. {text} Kiểm tra và bấm \"Đã nhận được hàng\" khi nhận.",
            $"/Staff/Claims/Details/{claim.Id}",
            cancellationToken);
    }

    public async Task ConfirmReceivedAsync(int claimId, string userId, string actorName, CancellationToken cancellationToken = default)
    {
        var claim = await GetOwnClaimAsync(claimId, userId, cancellationToken);
        if (claim.Status != ClaimStatus.ReturningToCustomer)
        {
            throw new InvalidOperationException("Chỉ xác nhận khi cửa hàng đã gửi sản phẩm thay thế cho bạn.");
        }

        ChangeStatus(claim, ClaimStatus.Completed, "Khách xác nhận đã nhận sản phẩm thay thế.", userId, actorName, byStaff: false, DateTime.UtcNow);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _notificationService.NotifyStaffAsync(
            $"Yêu cầu {claim.Code} đã hoàn tất",
            $"{actorName} xác nhận đã nhận sản phẩm thay thế ({claim.OrderItem?.ProductName}).",
            $"/Staff/Claims/Details/{claim.Id}",
            cancellationToken);
    }

    private async Task<WarrantyClaim> GetOwnClaimAsync(int claimId, string userId, CancellationToken cancellationToken)
    {
        var claim = await _claimRepository.GetDetailsAsync(claimId, cancellationToken);
        return claim is null || claim.UserId != userId ? throw new KeyNotFoundException("Không tìm thấy yêu cầu.") : claim;
    }

    #endregion

    #region Staff

    public async Task<ClaimListViewModel> GetForStaffAsync(
        ClaimStatus? status,
        ClaimType? type,
        string? search,
        CancellationToken cancellationToken = default)
    {
        var normalizedSearch = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
        var claims = await _claimRepository.GetForStaffAsync(status, type, normalizedSearch, cancellationToken);
        return new ClaimListViewModel
        {
            Claims = claims.Select(ToRow).ToList(),
            Status = status,
            Type = type,
            Search = normalizedSearch,
            OpenCount = await _claimRepository.CountOpenAsync(cancellationToken)
        };
    }

    public async Task<ClaimDetailsViewModel?> GetDetailsForStaffAsync(int claimId, CancellationToken cancellationToken = default)
    {
        var claim = await _claimRepository.GetDetailsAsync(claimId, cancellationToken);
        return claim is null ? null : ToDetails(claim, forStaff: true);
    }

    public async Task DecideAsync(
        int claimId,
        string actorUserId,
        string actorName,
        ClaimDecisionViewModel decision,
        CancellationToken cancellationToken = default)
    {
        var changed = await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var claim = await _claimRepository.GetDetailsAsync(claimId, token)
                ?? throw new KeyNotFoundException("Không tìm thấy yêu cầu.");

            if (!AllowedNextStatuses(claim).Contains(decision.NextStatus))
            {
                throw new InvalidOperationException("Không thể chuyển yêu cầu sang trạng thái đã chọn.");
            }

            var note = string.IsNullOrWhiteSpace(decision.Note) ? null : decision.Note.Trim();
            if (decision.NextStatus is ClaimStatus.NeedMoreInfo or ClaimStatus.Rejected && note is null)
            {
                throw new InvalidOperationException(decision.NextStatus == ClaimStatus.Rejected
                    ? "Vui lòng ghi lý do từ chối để thông báo cho khách hàng."
                    : "Vui lòng ghi rõ khách cần bổ sung minh chứng gì.");
            }

            if (decision.NextStatus == ClaimStatus.Approved && note is null && string.IsNullOrWhiteSpace(_store.Address))
            {
                throw new InvalidOperationException(
                    "Chưa cấu hình địa chỉ cửa hàng (mục Store:Address). Vui lòng ghi rõ địa chỉ, người nhận và số điện thoại để khách gửi hàng về.");
            }

            var now = DateTime.UtcNow;
            // Facts the customer must see (shipment details) go before the free-text message.
            string? facts = null;
            switch (decision.NextStatus)
            {
                case ClaimStatus.ReturningToCustomer:
                    facts = await ShipReplacementAsync(claim, decision, now, token);
                    break;
                case ClaimStatus.Completed:
                    facts = "Nhân viên xác nhận khách đã nhận sản phẩm thay thế.";
                    break;
                case ClaimStatus.Rejected when ItemAtShop(claim):
                    SetOutbound(claim, decision, now);
                    facts = $"Sản phẩm của khách được gửi trả {ClaimDisplay.ShipmentText(claim.OutboundCarrier, claim.OutboundTrackingCode)}.";
                    break;
            }

            var fullNote = facts is null ? note : note is null ? facts : $"{facts} {note}";
            ChangeStatus(claim, decision.NextStatus, fullNote, actorUserId, actorName, byStaff: true, now);
            return (claim.Id, claim.Code, claim.UserId, claim.Status, claim.Resolution, StaffMessage: fullNote);
        }, cancellationToken);

        // Only after the change is committed: tell the customer (in-site, real time).
        await _notificationService.NotifyClaimStatusChangedAsync(
            changed.Id, changed.Code, changed.UserId, changed.Status, changed.Resolution,
            changed.StaffMessage, cancellationToken);
    }

    public async Task SaveStaffNoteAsync(int claimId, string? note, CancellationToken cancellationToken = default)
    {
        var claim = await _claimRepository.GetByIdAsync(claimId, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy yêu cầu.");
        var trimmed = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        claim.StaffNote = trimmed is { Length: > 2000 } ? trimmed[..2000] : trimmed;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task SendMessageAsync(
        int claimId,
        string actorUserId,
        string actorName,
        string message,
        CancellationToken cancellationToken = default)
    {
        var claim = await _claimRepository.GetDetailsAsync(claimId, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy yêu cầu.");
        if (ClaimDisplay.IsClosed(claim.Status))
        {
            throw new InvalidOperationException("Yêu cầu đã đóng.");
        }

        var text = Limit(message, 2000) ?? throw new InvalidOperationException("Vui lòng nhập nội dung lời nhắn.");
        AddUpdate(claim, text, actorUserId, actorName, byStaff: true, DateTime.UtcNow);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _notificationService.NotifyClaimMessageAsync(claim.Id, claim.Code, claim.UserId, text, cancellationToken);
    }

    public async Task<(string Path, string ContentType, string FileName)?> GetMediaFileAsync(
        int mediaId,
        string? userId,
        CancellationToken cancellationToken = default)
    {
        var media = await _claimRepository.GetMediaAsync(mediaId, cancellationToken);
        if (media?.Claim is null || (userId is not null && media.Claim.UserId != userId))
        {
            return null;
        }

        var path = _mediaStorage.GetPath(media.ClaimId, media.StoredFileName);
        return path is null ? null : (path, media.ContentType, media.OriginalFileName);
    }

    #endregion

    #region Rules

    /// <summary>
    /// Staff workflow: review → (more info) → approve → receive item → (brand assessment) →
    /// ship a new product → customer receives (completed). No refunds: every claim ends with a new product or a rejection.
    /// Rejecting after the item reached the shop requires sending it back.
    /// </summary>
    private static IReadOnlyList<ClaimStatus> AllowedNextStatuses(WarrantyClaim claim) => claim.Status switch
    {
        ClaimStatus.Submitted => [ClaimStatus.Approved, ClaimStatus.NeedMoreInfo, ClaimStatus.Rejected],
        ClaimStatus.NeedMoreInfo => [ClaimStatus.Approved, ClaimStatus.Rejected],
        ClaimStatus.Approved => [ClaimStatus.Received, ClaimStatus.Rejected],
        ClaimStatus.Received when claim.Type == ClaimType.Warranty => [ClaimStatus.SentToBrand, ClaimStatus.ReturningToCustomer, ClaimStatus.Rejected],
        ClaimStatus.Received => [ClaimStatus.ReturningToCustomer, ClaimStatus.Rejected],
        ClaimStatus.SentToBrand => [ClaimStatus.ReturningToCustomer, ClaimStatus.Rejected],
        ClaimStatus.ReturningToCustomer => [ClaimStatus.Completed],
        _ => []
    };

    /// <summary>What can be shipped back: receipt defect → shop exchange; warranty → brand replacement or shop exchange.</summary>
    private static IReadOnlyList<ClaimResolution> ReplacementResolutions(ClaimType type) => type == ClaimType.ReceiptDefect
        ? [ClaimResolution.ExchangedByShop]
        : [ClaimResolution.ReplacedByBrand, ClaimResolution.ExchangedByShop];

    private static bool ItemAtShop(WarrantyClaim claim) => claim.Status is ClaimStatus.Received or ClaimStatus.SentToBrand;

    private static bool CustomerCanCancel(WarrantyClaim claim) =>
        claim.Status is ClaimStatus.Submitted or ClaimStatus.NeedMoreInfo
        || (claim.Status == ClaimStatus.Approved && !claim.ReturnShippedAt.HasValue);

    /// <summary>Exchange / brand replacement: takes stock for a shop exchange and records the shipment to the customer.</summary>
    private async Task<string> ShipReplacementAsync(WarrantyClaim claim, ClaimDecisionViewModel decision, DateTime now, CancellationToken token)
    {
        if (decision.Resolution is not { } resolution || !ReplacementResolutions(claim.Type).Contains(resolution))
        {
            throw new InvalidOperationException(claim.Type == ClaimType.ReceiptDefect
                ? "Lỗi khi nhận hàng chỉ gửi thay thế bằng sản phẩm mới từ kho cửa hàng."
                : "Vui lòng chọn: hãng thay thế hoặc cửa hàng đổi sản phẩm mới.");
        }

        SetOutbound(claim, decision, now);
        var variant = claim.OrderItem?.ProductVariant
            ?? throw new InvalidOperationException("Không tìm thấy phiên bản sản phẩm.");
        if (resolution == ClaimResolution.ExchangedByShop)
        {
            if (variant.StockQuantity < claim.Quantity)
            {
                throw new InvalidOperationException(
                    $"Không đủ tồn kho để đổi (còn {variant.StockQuantity}). Hãy nhập thêm hàng rồi gửi sản phẩm mới cho khách"
                    + " (cửa hàng không hoàn tiền).");
            }

            variant.StockQuantity -= claim.Quantity;
            await _claimRepository.AddInventoryTransactionAsync(new InventoryTransaction
            {
                ProductVariantId = variant.Id,
                Type = InventoryTransactionType.ClaimReplacement,
                Quantity = -claim.Quantity,
                Note = $"Xuất kho đổi hàng cho yêu cầu {claim.Code}",
                CreatedAt = now
            }, token);
        }

        claim.Resolution = resolution;
        var summary = $"Cách giải quyết: {ClaimDisplay.ResolutionText(resolution)}. Đã gửi sản phẩm thay thế "
            + $"{ClaimDisplay.ShipmentText(claim.OutboundCarrier, claim.OutboundTrackingCode)}.";
        return summary + await RestockIfRequestedAsync(claim, decision, variant, now, token);
    }

    /// <summary>Only a receipt-defect return (e.g. wrong item delivered, still sealed) goes back on sale; defective goods never do.</summary>
    private async Task<string> RestockIfRequestedAsync(
        WarrantyClaim claim, ClaimDecisionViewModel decision, ProductVariant variant, DateTime now, CancellationToken token)
    {
        if (!decision.RestockReturnedItem || claim.Type != ClaimType.ReceiptDefect)
        {
            return string.Empty;
        }

        variant.StockQuantity += claim.Quantity;
        await _claimRepository.AddInventoryTransactionAsync(new InventoryTransaction
        {
            ProductVariantId = variant.Id,
            Type = InventoryTransactionType.ClaimRestock,
            Quantity = claim.Quantity,
            Note = $"Nhập lại kho hàng khách trả về (còn bán được) – yêu cầu {claim.Code}",
            CreatedAt = now
        }, token);
        return " Hàng khách trả về đã được nhập lại kho.";
    }

    private static void SetOutbound(WarrantyClaim claim, ClaimDecisionViewModel decision, DateTime now)
    {
        var (carrier, tracking) = NormalizeShipment(decision.OutboundCarrier, decision.OutboundTrackingCode);
        claim.OutboundCarrier = carrier;
        claim.OutboundTrackingCode = tracking;
        claim.OutboundShippedAt = now;
    }

    /// <summary>Carrier is required; a tracking code is required unless the item is handed over at the shop.</summary>
    private static (string Carrier, string? Tracking) NormalizeShipment(string? carrier, string? trackingCode)
    {
        var name = Limit(carrier, 100) ?? throw new InvalidOperationException("Vui lòng chọn hoặc nhập đơn vị vận chuyển.");
        if (ClaimDisplay.IsInPerson(name))
        {
            return (ClaimDisplay.InPerson, null);
        }

        var tracking = Limit(trackingCode, 100) ?? throw new InvalidOperationException("Vui lòng nhập mã vận đơn.");
        return (name, tracking);
    }

    private static string? Limit(string? value, int max)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed.Length > max ? trimmed[..max] : trimmed;
    }

    /// <summary>Records information / a message without changing the status.</summary>
    private static void AddUpdate(WarrantyClaim claim, string note, string actorUserId, string actorName, bool byStaff, DateTime now)
    {
        claim.History.Add(NewHistory(claim.Status, claim.Status, note, actorUserId, actorName, byStaff, now));
        claim.UpdatedAt = now;
    }

    private static void EnsureDelivered(Order order)
    {
        if (order.Status != OrderStatus.Completed)
        {
            throw new InvalidOperationException("Chỉ gửi yêu cầu bảo hành/đổi hàng cho đơn đã giao thành công.");
        }
    }

    private static DateTime DeliveredAt(Order order) => order.PaidAt ?? order.UpdatedAt;

    /// <summary>
    /// Deadlines are inclusive local (Vietnam) dates: delivered on day D with N days → valid through the end of D + N.
    /// </summary>
    private static (int ReceiptDays, DateTime ReceiptDeadline, int WarrantyDays, DateTime? WarrantyDeadline) ComputeWindow(
        Order order, Brand brand, WarrantyPolicy? policy)
    {
        var deliveredLocal = DateOnly.FromDateTime(ShopClock.ToLocal(DeliveredAt(order)));
        DateTime EndOf(int days) => ShopClock.StartOfDayUtc(deliveredLocal.AddDays(days + 1)).AddSeconds(-1);

        var warrantyDays = policy?.WarrantyDays ?? 0;
        return (brand.ReceiptDefectDays, EndOf(brand.ReceiptDefectDays), warrantyDays, warrantyDays > 0 ? EndOf(warrantyDays) : null);
    }

    /// <summary>The order's lines with their receipt-defect / warranty deadlines and open claim.</summary>
    private static List<ClaimItemOptionViewModel> BuildItemOptions(
        Order order,
        IReadOnlyList<WarrantyPolicy> policies,
        IReadOnlyDictionary<int, string> openClaims,
        DateTime now)
    {
        return order.OrderItems.Where(item => item.ProductVariant?.Product is not null).Select(item =>
        {
            var product = item.ProductVariant!.Product!;
            var policy = policies.FirstOrDefault(row => row.BrandId == product.BrandId && row.CategoryId == product.CategoryId);
            var window = ComputeWindow(order, product.Brand!, policy);
            return new ClaimItemOptionViewModel
            {
                OrderItemId = item.Id,
                ProductName = item.ProductName,
                SKU = item.SKU,
                ImageUrl = product.ProductImages.FirstOrDefault()?.ImageUrl,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name ?? string.Empty,
                BrandName = product.Brand?.Name ?? string.Empty,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                ReceiptDefectDays = window.ReceiptDays,
                ReceiptDeadline = window.ReceiptDeadline,
                WarrantyDays = window.WarrantyDays,
                WarrantyDeadline = window.WarrantyDeadline,
                ReceiptWindowOpen = now <= window.ReceiptDeadline,
                WarrantyWindowOpen = window.WarrantyDeadline.HasValue && now <= window.WarrantyDeadline,
                OpenClaimCode = openClaims.GetValueOrDefault(item.Id)
            };
        }).ToList();
    }

    private async Task FillOptionsAsync(CreateClaimViewModel model, Order order, CancellationToken cancellationToken)
    {
        var lines = order.OrderItems.Where(item => item.ProductVariant?.Product is not null).ToList();
        var brandIds = lines.Select(item => item.ProductVariant!.Product!.BrandId).Distinct().ToList();
        var categoryIds = lines.Select(item => item.ProductVariant!.Product!.CategoryId).Distinct().ToList();
        var policies = await _claimRepository.GetPoliciesAsync(brandIds, cancellationToken);
        var defects = await _claimRepository.GetActiveDefectTypesAsync(categoryIds, cancellationToken);
        var openClaims = await _claimRepository.GetOpenClaimCodesAsync(lines.Select(item => item.Id).ToList(), cancellationToken);

        model.OrderCode = order.OrderCode;
        model.DeliveredAt = DeliveredAt(order);
        model.Limits = _mediaStorage.Limits;
        model.Items = BuildItemOptions(order, policies, openClaims, DateTime.UtcNow);
        model.Defects = defects.Select(defect => new DefectOptionViewModel
        {
            Id = defect.Id,
            Code = defect.Code,
            CategoryId = defect.CategoryId,
            Name = defect.Name,
            Recognition = defect.Recognition,
            EvidenceGuide = defect.EvidenceGuide,
            Coverage = defect.Coverage,
            Source = defect.Source,
            SourceUrl = defect.SourceUrl
        }).ToList();
    }

    private async Task<List<(IFormFile File, ClaimMediaKind Kind)>> ValidateFilesAsync(
        IReadOnlyList<IFormFile> files,
        bool requireVideo,
        CancellationToken cancellationToken)
    {
        var validated = new List<(IFormFile File, ClaimMediaKind Kind)>(files.Count);
        foreach (var file in files)
        {
            var (kind, error) = await _mediaStorage.ValidateAsync(file, cancellationToken);
            if (error is not null)
            {
                throw new InvalidOperationException(error);
            }

            validated.Add((file, kind!.Value));
        }

        var videos = validated.Count(entry => entry.Kind == ClaimMediaKind.Video);
        var images = validated.Count - videos;
        if (requireVideo && videos == 0)
        {
            throw new InvalidOperationException("Vui lòng tải lên ít nhất một video minh chứng tình trạng sản phẩm.");
        }

        if (videos > Limits.MaxVideos || images > Limits.MaxImages)
        {
            throw new InvalidOperationException($"Mỗi lần gửi tối đa {Limits.MaxVideos} video và {Limits.MaxImages} ảnh.");
        }

        return validated;
    }

    private async Task AttachFilesAsync(
        WarrantyClaim claim,
        IEnumerable<(IFormFile File, ClaimMediaKind Kind)> files,
        string userId,
        bool byStaff,
        DateTime now,
        List<(int ClaimId, string Name)> savedFiles,
        CancellationToken token)
    {
        foreach (var (file, kind) in files)
        {
            var storedName = await _mediaStorage.SaveAsync(claim.Id, file, token);
            savedFiles.Add((claim.Id, storedName));
            var originalName = Path.GetFileName(file.FileName);
            claim.Media.Add(new WarrantyClaimMedia
            {
                Kind = kind,
                StoredFileName = storedName,
                OriginalFileName = originalName.Length > 256 ? originalName[^256..] : originalName,
                ContentType = ClaimMediaStorage.ContentTypeFor(file.FileName),
                SizeBytes = file.Length,
                UploadedByUserId = userId,
                UploadedByStaff = byStaff,
                UploadedAt = now
            });
        }
    }

    private static void ChangeStatus(WarrantyClaim claim, ClaimStatus next, string? note, string actorUserId, string actorName, bool byStaff, DateTime now)
    {
        claim.History.Add(NewHistory(claim.Status, next, note, actorUserId, actorName, byStaff, now));
        claim.Status = next;
        claim.UpdatedAt = now;
        if (ClaimDisplay.IsClosed(next))
        {
            claim.ClosedAt = now;
        }
    }

    private static WarrantyClaimHistory NewHistory(ClaimStatus? from, ClaimStatus to, string? note, string actorUserId, string actorName, bool byStaff, DateTime now) => new()
    {
        FromStatus = from,
        ToStatus = to,
        Note = note is { Length: > 2000 } ? note[..2000] : note,
        ActorUserId = actorUserId,
        ActorName = actorName.Length > 256 ? actorName[..256] : actorName,
        ByStaff = byStaff,
        CreatedAt = now
    };

    private static string ToLocalText(DateTime utc) => ShopClock.ToLocal(utc).ToString("dd/MM/yyyy");

    #endregion

    #region Mapping

    private static ClaimRowViewModel ToRow(WarrantyClaim claim) => new()
    {
        Id = claim.Id,
        Code = claim.Code,
        CreatedAt = claim.CreatedAt,
        UpdatedAt = claim.UpdatedAt,
        ProductName = claim.OrderItem?.ProductName ?? string.Empty,
        DefectName = claim.DefectType is null ? string.Empty : $"{claim.DefectType.Code} – {claim.DefectType.Name}",
        Type = claim.Type,
        Status = claim.Status,
        CustomerName = claim.User?.FullName ?? claim.User?.UserName,
        OrderCode = claim.Order?.OrderCode
    };

    private ClaimDetailsViewModel ToDetails(WarrantyClaim claim, bool forStaff)
    {
        var item = claim.OrderItem!;
        return new ClaimDetailsViewModel
        {
            Id = claim.Id,
            Code = claim.Code,
            Type = claim.Type,
            Status = claim.Status,
            CreatedAt = claim.CreatedAt,
            DeadlineAt = claim.DeadlineAt,
            ClosedAt = claim.ClosedAt,
            OrderId = claim.OrderId,
            OrderCode = claim.Order?.OrderCode ?? string.Empty,
            DeliveredAt = claim.Order is null ? claim.CreatedAt : DeliveredAt(claim.Order),
            ProductName = item.ProductName,
            SKU = item.SKU,
            ProductVariantId = item.ProductVariantId,
            UnitPrice = item.UnitPrice,
            Quantity = claim.Quantity,
            DefectCode = claim.DefectType?.Code ?? string.Empty,
            DefectName = claim.DefectType?.Name ?? string.Empty,
            EvidenceGuide = claim.DefectType?.EvidenceGuide,
            Description = claim.Description,
            SerialNumber = claim.SerialNumber,
            CustomerName = claim.User?.FullName ?? claim.Order?.RecipientName,
            CustomerAccountPhone = forStaff ? claim.User?.PhoneNumber ?? claim.User?.UserName : null,
            CustomerPhone = forStaff ? claim.Order?.RecipientPhone : null,
            Resolution = claim.Resolution,
            StaffNote = forStaff ? claim.StaffNote : null,
            ReturnCarrier = claim.ReturnCarrier,
            ReturnTrackingCode = claim.ReturnTrackingCode,
            ReturnShippedAt = claim.ReturnShippedAt,
            OutboundCarrier = claim.OutboundCarrier,
            OutboundTrackingCode = claim.OutboundTrackingCode,
            OutboundShippedAt = claim.OutboundShippedAt,
            DeliveryName = claim.Order?.RecipientName,
            DeliveryPhone = claim.Order?.RecipientPhone,
            DeliveryAddress = claim.Order?.ShippingAddress,
            ShopAddress = string.IsNullOrWhiteSpace(_store.Address) ? null : _store.Address.Trim(),
            ShopHotline = string.IsNullOrWhiteSpace(_store.Hotline) ? null : _store.Hotline.Trim(),
            Media = claim.Media.Select(media => new ClaimMediaViewModel
            {
                Id = media.Id,
                Kind = media.Kind,
                OriginalFileName = media.OriginalFileName,
                SizeBytes = media.SizeBytes,
                UploadedByStaff = media.UploadedByStaff,
                UploadedAt = media.UploadedAt
            }).ToList(),
            History = claim.History.OrderBy(entry => entry.CreatedAt).Select(entry => new ClaimHistoryViewModel
            {
                FromStatus = entry.FromStatus,
                ToStatus = entry.ToStatus,
                Note = entry.Note,
                ActorName = entry.ByStaff && !forStaff ? "Cửa hàng PôpPôp" : entry.ActorName,
                ByStaff = entry.ByStaff,
                CreatedAt = entry.CreatedAt
            }).ToList(),
            CanCancel = !forStaff && CustomerCanCancel(claim),
            CanAddEvidence = !forStaff && claim.Status == ClaimStatus.NeedMoreInfo,
            CanReportShipment = !forStaff && claim.Status == ClaimStatus.Approved,
            CanConfirmReceived = !forStaff && claim.Status == ClaimStatus.ReturningToCustomer,
            AllowedNextStatuses = forStaff ? AllowedNextStatuses(claim) : [],
            ItemAtShop = ItemAtShop(claim),
            CanMessage = forStaff && !ClaimDisplay.IsClosed(claim.Status),
            AvailableStock = item.ProductVariant?.StockQuantity ?? 0,
            Limits = _mediaStorage.Limits
        };
    }

    #endregion
}
