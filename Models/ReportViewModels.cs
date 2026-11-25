using PopPop.Data.Entities.Enums;

namespace PopPop.Models;

// ---------- Raw rows returned by the report repository (UTC timestamps) ----------

public sealed record ReportOrderRow(int Id, string OrderCode, DateTime CreatedAt, DateTime? CompletedAt,
    OrderStatus Status, decimal TotalAmount, decimal ShippingFee, string? CustomerName,
    string? RecipientPhone, int ItemCount);

public sealed record ReportSoldItemRow(int ProductId, string ProductName, string CategoryName, int Quantity, decimal Revenue);

public sealed record LowStockRow(int ProductId, string ProductName, string SKU, string? VariantLabel, int StockQuantity, bool ProductActive);

// ---------- Page model ----------

public sealed class ReportViewModel
{
    public DateOnly From { get; init; }
    public DateOnly To { get; init; }
    public string? Preset { get; init; }
    public bool GroupByMonth { get; init; }

    public decimal Revenue { get; init; }
    public decimal ShippingRevenue { get; init; }
    public decimal PreviousRevenue { get; init; }
    public int OrdersPlaced { get; init; }
    public int OrdersCompleted { get; init; }
    public int OrdersCancelled { get; init; }
    public int ItemsSold { get; init; }
    public int NewCustomers { get; init; }
    public decimal AverageOrderValue => OrdersCompleted == 0 ? 0m : Math.Round(Revenue / OrdersCompleted);
    public double CancellationRate => OrdersPlaced == 0 ? 0 : Math.Round(OrdersCancelled * 100.0 / OrdersPlaced, 1);

    /// <summary>Revenue change vs. the previous period of the same length, in percent (null when there is nothing to compare).</summary>
    public double? RevenueChangePercent => PreviousRevenue == 0 ? null : Math.Round((double)((Revenue - PreviousRevenue) / PreviousRevenue * 100m), 1);

    public IReadOnlyList<ReportTimePoint> Timeline { get; init; } = [];
    public IReadOnlyList<ReportProductRow> TopProducts { get; init; } = [];
    public IReadOnlyList<ReportCategoryRow> Categories { get; init; } = [];
    public IReadOnlyList<LowStockRow> LowStock { get; init; } = [];
    public int LowStockThreshold { get; init; }
}

public sealed record ReportTimePoint(string Label, DateOnly Start, decimal Revenue, int OrdersPlaced, int OrdersCompleted);

public sealed record ReportProductRow(int ProductId, string ProductName, string CategoryName, int Quantity, decimal Revenue);

public sealed record ReportCategoryRow(string CategoryName, int Quantity, decimal Revenue, double SharePercent);

/// <summary>A downloadable file produced by the report service.</summary>
public sealed record ReportFile(string FileName, string ContentType, byte[] Content);
