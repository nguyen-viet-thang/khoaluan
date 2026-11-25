using PopPop.Models;

namespace PopPop.Services.Interfaces;

/// <summary>
/// Sales reports. Revenue = orders delivered (completed) in the period, i.e. money actually collected (COD).
/// Dates are Vietnam local dates, both ends inclusive.
/// </summary>
public interface IReportService
{
    /// <param name="preset">7d, 30d, this-month, last-month, this-year; ignored when from/to are given.</param>
    Task<ReportViewModel> GetReportAsync(DateOnly? from, DateOnly? to, string? preset, CancellationToken cancellationToken = default);

    /// <summary>Excel workbook with summary, revenue timeline, products, categories, orders and low-stock sheets.</summary>
    Task<ReportFile> ExportAsync(DateOnly? from, DateOnly? to, string? preset, CancellationToken cancellationToken = default);
}
