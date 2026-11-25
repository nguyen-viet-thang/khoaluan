using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PopPop.Data.Entities;
using PopPop.Models;
using PopPop.Services.Interfaces;

namespace PopPop.Controllers;

/// <summary>Customer side of warranty / receipt-defect claims.</summary>
[Authorize]
public sealed class ClaimsController : Controller
{
    /// <summary>3 videos × 100 MB + 5 photos × 5 MB + form overhead.</summary>
    private const long UploadRequestLimit = 340L * 1024 * 1024;

    private readonly IWarrantyClaimService _claimService;
    private readonly UserManager<User> _userManager;

    public ClaimsController(IWarrantyClaimService claimService, UserManager<User> userManager)
    {
        _claimService = claimService;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var claims = await _claimService.GetForCustomerAsync(UserId, cancellationToken: cancellationToken);
        return View(claims);
    }

    /// <summary>Step 1: the customer's delivered orders with their products; picking a product opens the claim form.</summary>
    [HttpGet]
    public async Task<IActionResult> Start(string? search, CancellationToken cancellationToken)
    {
        var model = await _claimService.GetStartAsync(UserId, search, cancellationToken);
        return View(model);
    }

    /// <summary>Step 2: defect, description and video evidence for the product picked on step 1.</summary>
    [HttpGet]
    public async Task<IActionResult> Create(int orderId, int? orderItemId, CancellationToken cancellationToken)
    {
        try
        {
            var model = await _claimService.GetCreateFormAsync(orderId, orderItemId, UserId, cancellationToken);
            return model is null ? NotFound() : View(model);
        }
        catch (InvalidOperationException exception)
        {
            TempData["OrderError"] = exception.Message;
            return RedirectToAction("Details", "Orders", new { id = orderId });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(UploadRequestLimit)]
    [RequestFormLimits(MultipartBodyLengthLimit = UploadRequestLimit)]
    public async Task<IActionResult> Create(CreateClaimViewModel model, List<IFormFile>? files, CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var claimId = await _claimService.CreateAsync(UserId, await GetActorNameAsync(), model, files ?? [], cancellationToken);
                TempData["ClaimSuccess"] = "Đã gửi yêu cầu. Cửa hàng sẽ xem minh chứng và phản hồi sớm nhất.";
                return RedirectToAction(nameof(Details), new { id = claimId });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException exception)
            {
                ModelState.AddModelError(string.Empty, exception.Message);
            }
        }

        await _claimService.PopulateCreateFormAsync(model, UserId, cancellationToken);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var model = await _claimService.GetDetailsForCustomerAsync(id, UserId, cancellationToken);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(UploadRequestLimit)]
    [RequestFormLimits(MultipartBodyLengthLimit = UploadRequestLimit)]
    public async Task<IActionResult> AddEvidence(int id, string? note, List<IFormFile>? files, CancellationToken cancellationToken)
    {
        try
        {
            await _claimService.AddEvidenceAsync(id, UserId, await GetActorNameAsync(), note, files ?? [], cancellationToken);
            TempData["ClaimSuccess"] = "Đã gửi minh chứng bổ sung.";
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException exception)
        {
            TempData["ClaimError"] = exception.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _claimService.CancelAsync(id, UserId, await GetActorNameAsync(), cancellationToken);
            TempData["ClaimSuccess"] = "Đã hủy yêu cầu.";
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException exception)
        {
            TempData["ClaimError"] = exception.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> ReportShipment(int id, ClaimShipmentInput input, CancellationToken cancellationToken) =>
        RunAsync(id, async () => await _claimService.ReportShipmentAsync(id, UserId, await GetActorNameAsync(), input, cancellationToken),
            "Đã lưu thông tin gửi hàng. Cửa hàng sẽ báo khi nhận được sản phẩm.");

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> ConfirmReceived(int id, CancellationToken cancellationToken) =>
        RunAsync(id, async () => await _claimService.ConfirmReceivedAsync(id, UserId, await GetActorNameAsync(), cancellationToken),
            "Cảm ơn bạn! Yêu cầu đã hoàn tất.");

    /// <summary>Runs a customer action and returns to the claim page with its outcome.</summary>
    private async Task<IActionResult> RunAsync(int id, Func<Task> action, string successMessage)
    {
        if (!ModelState.IsValid)
        {
            TempData["ClaimError"] = ModelState.Values.SelectMany(entry => entry.Errors).First().ErrorMessage;
            return RedirectToAction(nameof(Details), new { id });
        }

        try
        {
            await action();
            TempData["ClaimSuccess"] = successMessage;
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException exception)
        {
            TempData["ClaimError"] = exception.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    /// <summary>Streams an evidence file of the customer's own claim (supports seeking in the video player).</summary>
    [HttpGet]
    public async Task<IActionResult> Media(int id, CancellationToken cancellationToken)
    {
        var file = await _claimService.GetMediaFileAsync(id, UserId, cancellationToken);
        if (file is null)
        {
            return NotFound();
        }

        Response.Headers.XContentTypeOptions = "nosniff";
        return PhysicalFile(file.Value.Path, file.Value.ContentType, enableRangeProcessing: true);
    }

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("Không xác định được tài khoản hiện tại.");

    private async Task<string> GetActorNameAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        return user?.FullName ?? user?.UserName ?? "Khách hàng";
    }
}
