using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PopPop.Models;
using PopPop.Services.Interfaces;

namespace PopPop.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public sealed class WarrantyPoliciesController : Controller
{
    private readonly IWarrantyPolicyService _policyService;

    public WarrantyPoliciesController(IWarrantyPolicyService policyService)
    {
        _policyService = policyService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        return View(await _policyService.GetMatrixAsync(cancellationToken));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateReceiptDays(int brandId, int days, CancellationToken cancellationToken)
    {
        try
        {
            await _policyService.UpdateReceiptDefectDaysAsync(brandId, days, cancellationToken);
            TempData["SuccessMessage"] = "Đã cập nhật thời hạn báo lỗi khi nhận hàng.";
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> EditPolicy(int brandId, int categoryId, CancellationToken cancellationToken)
    {
        var model = await _policyService.GetPolicyFormAsync(brandId, categoryId, cancellationToken);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPolicy(WarrantyPolicyFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var current = await _policyService.GetPolicyFormAsync(model.BrandId, model.CategoryId, cancellationToken);
            if (current is null)
            {
                return NotFound();
            }

            model.BrandName = current.BrandName;
            model.CategoryName = current.CategoryName;
            return View(model);
        }

        try
        {
            await _policyService.SavePolicyAsync(model, cancellationToken);
            TempData["SuccessMessage"] = "Đã lưu chính sách bảo hành.";
            return RedirectToAction(nameof(Index));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet]
    public async Task<IActionResult> EditDefect(int id, CancellationToken cancellationToken)
    {
        var model = await _policyService.GetDefectFormAsync(id, cancellationToken);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditDefect(int id, DefectTypeFormViewModel model, CancellationToken cancellationToken)
    {
        model.Id = id;
        if (!ModelState.IsValid)
        {
            var current = await _policyService.GetDefectFormAsync(id, cancellationToken);
            if (current is null)
            {
                return NotFound();
            }

            model.Code = current.Code;
            model.CategoryName = current.CategoryName;
            return View(model);
        }

        try
        {
            await _policyService.SaveDefectAsync(model, cancellationToken);
            TempData["SuccessMessage"] = $"Đã lưu loại lỗi.";
            return RedirectToAction(nameof(Index), null, "defects");
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
