using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PopPop.Services.Interfaces;

namespace PopPop.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public sealed class ReportsController : Controller
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(DateOnly? from, DateOnly? to, string? preset, CancellationToken cancellationToken)
    {
        var model = await _reportService.GetReportAsync(from, to, preset, cancellationToken);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Export(DateOnly? from, DateOnly? to, string? preset, CancellationToken cancellationToken)
    {
        var file = await _reportService.ExportAsync(from, to, preset, cancellationToken);
        return File(file.Content, file.ContentType, file.FileName);
    }
}
