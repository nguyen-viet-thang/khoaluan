using PopPop.Models;

namespace PopPop.Repositories.Interfaces;

/// <summary>Read-only queries for the reports page. All ranges are [fromUtc, toUtc).</summary>
public interface IReportRepository
{
    /// <summary>Orders placed in the range, plus orders completed (delivered) in the range.</summary>
    Task<IReadOnlyList<ReportOrderRow>> GetOrdersAsync(DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default);

    /// <summary>Order lines of orders completed in the range.</summary>
    Task<IReadOnlyList<ReportSoldItemRow>> GetSoldItemsAsync(DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default);

    /// <summary>Revenue of orders completed in the range.</summary>
    Task<decimal> GetCompletedRevenueAsync(DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default);

    Task<int> CountNewCustomersAsync(DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default);

    /// <summary>Active variants of active products whose stock is at or below the threshold.</summary>
    Task<IReadOnlyList<LowStockRow>> GetLowStockAsync(int threshold, CancellationToken cancellationToken = default);
}
