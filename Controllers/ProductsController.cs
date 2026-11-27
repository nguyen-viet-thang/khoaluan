using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PopPop.Services.Interfaces;

namespace PopPop.Controllers;

[AllowAnonymous]
public class ProductsController : Controller
{
    private const int MaxSpecIds = 30;
    private const int LiveResultCount = 6;
    private readonly IProductService _productService;
    private readonly IRacketAdvisorService _advisor;

    public ProductsController(IProductService productService, IRacketAdvisorService advisor)
    {
        _productService = productService;
        _advisor = advisor;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        int page = 1,
        int? categoryId = null,
        int? brandId = null,
        string? q = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string? sort = null,
        [FromQuery(Name = "spec")] int[]? spec = null,
        CancellationToken cancellationToken = default)
    {
        var model = await _productService.GetCatalogAsync(
            page,
            categoryId,
            brandId,
            q,
            minPrice,
            maxPrice,
            sort,
            LimitSpecIds(spec),
            cancellationToken);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var product = await _productService.GetDetailsAsync(id, cancellationToken);
        if (product is null)
        {
            return NotFound();
        }

        // Racket pages: how well this racket fits the player's saved advisor profile (or an invitation to create one).
        if (AdvisorController.ReadSavedProfile(Request) is { } profile)
        {
            ViewData["RacketFit"] = await _advisor.EvaluateAsync(profile, id, cancellationToken);
        }
        else
        {
            ViewData["IsRacket"] = await _advisor.IsRacketAsync(id, cancellationToken);
        }

        return View(product);
    }

    /// <summary>Live results for the header search panel.</summary>
    [HttpGet]
    public async Task<IActionResult> Search(
        string? q,
        int? categoryId,
        int? brandId,
        decimal? minPrice,
        decimal? maxPrice,
        [FromQuery(Name = "spec")] int[]? spec,
        CancellationToken cancellationToken)
    {
        var results = await _productService.SearchAsync(
            categoryId, brandId, q, minPrice, maxPrice, LimitSpecIds(spec), LiveResultCount,
            Url.Action(nameof(Index), "Products") ?? "/Products", cancellationToken);
        return PartialView("_SearchResults", results);
    }

    /// <summary>Brand and specification chips of a category for the header search panel.</summary>
    [HttpGet]
    public async Task<IActionResult> SearchFilters(int? categoryId, CancellationToken cancellationToken)
    {
        var filters = await _productService.GetSearchFiltersAsync(categoryId, cancellationToken);
        return PartialView("_SearchFilters", filters);
    }

    private static int[] LimitSpecIds(int[]? spec) =>
        spec is null ? [] : spec.Where(id => id > 0).Distinct().Take(MaxSpecIds).ToArray();
}
