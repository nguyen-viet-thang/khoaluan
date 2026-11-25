using Microsoft.AspNetCore.Mvc;
using PopPop.Data.Entities;
using PopPop.Models;
using PopPop.Services.Interfaces;

namespace PopPop.Areas.Admin.Controllers;

/// <summary>
/// Shared CRUD for product groupings. Concrete controllers (Categories, Brands) only supply labels.
/// </summary>
public abstract class CatalogGroupController<TEntity> : Controller
    where TEntity : class, ICatalogGroupEntity
{
    private const string IndexView = "~/Areas/Admin/Views/Shared/CatalogGroup/Index.cshtml";
    private const string FormView = "~/Areas/Admin/Views/Shared/CatalogGroup/Form.cshtml";

    private readonly ICatalogGroupService<TEntity> _service;

    protected CatalogGroupController(ICatalogGroupService<TEntity> service)
    {
        _service = service;
    }

    /// <summary>Lower-case noun, e.g. "danh mục".</summary>
    protected abstract string GroupLabel { get; }

    /// <summary>Plural page title, e.g. "Quản lý danh mục".</summary>
    protected abstract string PageTitle { get; }

    protected abstract string IconClass { get; }

    [HttpGet]
    public async Task<IActionResult> Index(string? search, string? status, CancellationToken cancellationToken)
    {
        SetLabels();
        var model = await _service.GetListAsync(search, status, cancellationToken);
        return View(IndexView, model);
    }

    [HttpGet]
    public IActionResult Create()
    {
        SetLabels();
        return View(FormView, new CatalogGroupFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CatalogGroupFormViewModel model, CancellationToken cancellationToken)
    {
        model.Id = null;
        if (ModelState.IsValid)
        {
            try
            {
                await _service.CreateAsync(model, cancellationToken);
                TempData["SuccessMessage"] = $"Đã thêm {GroupLabel} \"{model.Name.Trim()}\".";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException exception)
            {
                ModelState.AddModelError(nameof(model.Name), exception.Message);
            }
        }

        SetLabels();
        return View(FormView, model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var model = await _service.GetFormAsync(id, cancellationToken);
        if (model is null)
        {
            return NotFound();
        }

        SetLabels();
        return View(FormView, model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CatalogGroupFormViewModel model, CancellationToken cancellationToken)
    {
        model.Id = id;
        if (ModelState.IsValid)
        {
            try
            {
                await _service.UpdateAsync(model, cancellationToken);
                TempData["SuccessMessage"] = $"Đã cập nhật {GroupLabel} \"{model.Name.Trim()}\".";
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException exception)
            {
                ModelState.AddModelError(nameof(model.Name), exception.Message);
            }
        }

        SetLabels();
        return View(FormView, model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _service.DeleteAsync(id, cancellationToken);
            TempData["SuccessMessage"] = $"Đã xóa {GroupLabel}.";
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

    private void SetLabels()
    {
        ViewData["GroupLabel"] = GroupLabel;
        ViewData["PageTitle"] = PageTitle;
        ViewData["IconClass"] = IconClass;
    }
}
