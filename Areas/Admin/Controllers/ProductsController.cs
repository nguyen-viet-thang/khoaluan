using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PopPop.Models;
using PopPop.Services.Interfaces;

namespace PopPop.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public sealed class ProductsController : Controller
{
    // 10 images x 5 MB + form overhead
    private const long UploadRequestLimit = 60 * 1024 * 1024;

    private readonly IProductManagementService _productManagementService;

    public ProductsController(IProductManagementService productManagementService)
    {
        _productManagementService = productManagementService;
    }

    #region Product

    [HttpGet]
    public async Task<IActionResult> Index(
        string? search,
        int? categoryId,
        int? brandId,
        string? status,
        CancellationToken cancellationToken)
    {
        var model = await _productManagementService.GetListAsync(search, categoryId, brandId, status, cancellationToken);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = new ProductFormViewModel();
        await _productManagementService.PopulateFormOptionsAsync(model, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(UploadRequestLimit)]
    [RequestFormLimits(MultipartBodyLengthLimit = UploadRequestLimit)]
    public async Task<IActionResult> Create(
        ProductFormViewModel model,
        List<IFormFile>? images,
        CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var productId = await _productManagementService.CreateAsync(model, images ?? [], cancellationToken);
                TempData["SuccessMessage"] = "Đã tạo sản phẩm. Hãy thêm phiên bản (SKU, giá, tồn kho) để sản phẩm hiển thị trên cửa hàng.";
                return RedirectToAction(nameof(Edit), new { id = productId });
            }
            catch (InvalidOperationException exception)
            {
                ModelState.AddModelError(string.Empty, exception.Message);
            }
        }

        await _productManagementService.PopulateFormOptionsAsync(model, cancellationToken);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var model = await _productManagementService.GetEditAsync(id, cancellationToken: cancellationToken);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind(Prefix = "Form")] ProductFormViewModel form,
        CancellationToken cancellationToken)
    {
        form.Id = id;
        if (ModelState.IsValid)
        {
            try
            {
                await _productManagementService.UpdateAsync(form, cancellationToken);
                TempData["SuccessMessage"] = "Đã lưu thông tin sản phẩm.";
                return RedirectToAction(nameof(Edit), new { id });
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

        var model = await _productManagementService.GetEditAsync(id, form, cancellationToken);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateSpecs(int id, [FromForm(Name = "spec")] int[]? spec, CancellationToken cancellationToken)
    {
        try
        {
            await _productManagementService.UpdateSpecsAsync(id, spec ?? [], cancellationToken);
            TempData["SuccessMessage"] = "Đã lưu thông số sản phẩm.";
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _productManagementService.DeleteAsync(id, cancellationToken);
            TempData["SuccessMessage"] = "Đã xóa sản phẩm.";
            return RedirectToAction(nameof(Index));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
            return RedirectToAction(nameof(Edit), new { id });
        }
    }

    #endregion

    #region Variant & inventory

    [HttpGet]
    public async Task<IActionResult> CreateVariant(int productId, CancellationToken cancellationToken)
    {
        var model = await _productManagementService.GetVariantFormAsync(productId, null, cancellationToken);
        return model is null ? NotFound() : View("VariantForm", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateVariant(ProductVariantFormViewModel model, CancellationToken cancellationToken)
    {
        model.VariantId = null;
        if (ModelState.IsValid)
        {
            try
            {
                await _productManagementService.CreateVariantAsync(model, cancellationToken);
                TempData["SuccessMessage"] = "Đã thêm phiên bản sản phẩm.";
                return RedirectToAction(nameof(Edit), new { id = model.ProductId });
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

        await _productManagementService.PopulateVariantSpecsAsync(model, cancellationToken);
        return View("VariantForm", model);
    }

    [HttpGet]
    public async Task<IActionResult> EditVariant(int productId, int id, CancellationToken cancellationToken)
    {
        var model = await _productManagementService.GetVariantFormAsync(productId, id, cancellationToken);
        return model is null ? NotFound() : View("VariantForm", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditVariant(int id, ProductVariantFormViewModel model, CancellationToken cancellationToken)
    {
        model.VariantId = id;
        ModelState.Remove(nameof(model.InitialStock));
        if (ModelState.IsValid)
        {
            try
            {
                await _productManagementService.UpdateVariantAsync(model, cancellationToken);
                TempData["SuccessMessage"] = "Đã cập nhật phiên bản sản phẩm.";
                return RedirectToAction(nameof(Edit), new { id = model.ProductId });
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

        await _productManagementService.PopulateVariantSpecsAsync(model, cancellationToken);
        return View("VariantForm", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteVariant(int id, int productId, CancellationToken cancellationToken)
    {
        try
        {
            productId = await _productManagementService.DeleteVariantAsync(id, cancellationToken);
            TempData["SuccessMessage"] = "Đã xóa phiên bản sản phẩm.";
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Edit), new { id = productId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AdjustStock(
        int productId,
        [Bind(Prefix = "StockAdjustment")] StockAdjustmentViewModel adjustment,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = string.Join(" ", ModelState.Values
                .SelectMany(entry => entry.Errors)
                .Select(error => error.ErrorMessage));
            return RedirectToAction(nameof(Edit), new { id = productId });
        }

        try
        {
            productId = await _productManagementService.AdjustStockAsync(adjustment, cancellationToken);
            TempData["SuccessMessage"] = "Đã cập nhật tồn kho.";
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Edit), new { id = productId });
    }

    #endregion

    #region Images

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(UploadRequestLimit)]
    [RequestFormLimits(MultipartBodyLengthLimit = UploadRequestLimit)]
    public async Task<IActionResult> UploadImages(int productId, List<IFormFile>? images, CancellationToken cancellationToken)
    {
        try
        {
            await _productManagementService.UploadImagesAsync(productId, images ?? [], cancellationToken);
            TempData["SuccessMessage"] = "Đã tải ảnh lên.";
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Edit), new { id = productId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetPrimaryImage(int id, CancellationToken cancellationToken)
    {
        try
        {
            var productId = await _productManagementService.SetPrimaryImageAsync(id, cancellationToken);
            TempData["SuccessMessage"] = "Đã đặt ảnh đại diện.";
            return RedirectToAction(nameof(Edit), new { id = productId });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteImage(int id, CancellationToken cancellationToken)
    {
        try
        {
            var productId = await _productManagementService.DeleteImageAsync(id, cancellationToken);
            TempData["SuccessMessage"] = "Đã xóa ảnh.";
            return RedirectToAction(nameof(Edit), new { id = productId });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    #endregion
}
