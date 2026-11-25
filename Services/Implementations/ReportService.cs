using PopPop.Data.Entities.Enums;
using PopPop.Models;
using PopPop.Repositories.Interfaces;
using PopPop.Services.Interfaces;

namespace PopPop.Services.Implementations;

public sealed class ReportService : IReportService
{
    private const int LowStockThreshold = 5;
    private const int TopProductCount = 10;
    private const int MaxRangeDays = 366 * 2;
    private const int MaxDailyPoints = 62;

    private readonly IReportRepository _reportRepository;

    public ReportService(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public async Task<ReportViewModel> GetReportAsync(
        DateOnly? from,
        DateOnly? to,
        string? preset,
        CancellationToken cancellationToken = default)
    {
        var data = await LoadAsync(from, to, preset, cancellationToken);
        return data.Model;
    }

    public async Task<ReportFile> ExportAsync(
        DateOnly? from,
        DateOnly? to,
        string? preset,
        CancellationToken cancellationToken = default)
    {
        var (model, orders, soldItems) = await LoadAsync(from, to, preset, cancellationToken);
        var period = $"{model.From:dd/MM/yyyy} - {model.To:dd/MM/yyyy}";

        var workbook = new XlsxWriter()
            .AddSheet("Tổng quan", ["Chỉ số", "Giá trị"],
            [
                ["Kỳ báo cáo", period],
                ["Doanh thu (đơn giao thành công)", model.Revenue],
                ["Trong đó phí giao hàng", model.ShippingRevenue],
                ["Doanh thu kỳ trước", model.PreviousRevenue],
                ["Số đơn đặt", model.OrdersPlaced],
                ["Số đơn giao thành công", model.OrdersCompleted],
                ["Số đơn bị hủy", model.OrdersCancelled],
                ["Tỷ lệ hủy (%)", model.CancellationRate],
                ["Giá trị đơn trung bình", model.AverageOrderValue],
                ["Số sản phẩm bán ra", model.ItemsSold],
                ["Khách hàng mới", model.NewCustomers]
            ], 34, 22)
            .AddSheet("Doanh thu theo thời gian", [model.GroupByMonth ? "Tháng" : "Ngày", "Doanh thu", "Số đơn đặt", "Số đơn giao thành công"],
                model.Timeline.Select(point => new object?[] { point.Label, point.Revenue, point.OrdersPlaced, point.OrdersCompleted }), 16, 18, 14, 22)
            .AddSheet("Sản phẩm bán ra", ["Sản phẩm", "Danh mục", "Số lượng", "Doanh thu"],
                AggregateProducts(soldItems).Select(row => new object?[] { row.ProductName, row.CategoryName, row.Quantity, row.Revenue }), 48, 22, 12, 18)
            .AddSheet("Doanh thu theo danh mục", ["Danh mục", "Số lượng", "Doanh thu", "Tỷ trọng (%)"],
                model.Categories.Select(row => new object?[] { row.CategoryName, row.Quantity, row.Revenue, row.SharePercent }), 26, 12, 18, 14)
            .AddSheet("Đơn hàng", ["Mã đơn", "Ngày đặt", "Ngày giao thành công", "Khách hàng", "Điện thoại", "Số SP", "Phí giao hàng", "Tổng tiền", "Trạng thái"],
                orders.Select(order => new object?[]
                {
                    order.OrderCode,
                    ShopClock.ToLocal(order.CreatedAt),
                    order.CompletedAt is { } completedAt ? ShopClock.ToLocal(completedAt) : null,
                    order.CustomerName,
                    order.RecipientPhone,
                    order.ItemCount,
                    order.ShippingFee,
                    order.TotalAmount,
                    StatusText(order.Status)
                }), 46, 17, 20, 24, 14, 8, 14, 14, 18)
            .AddSheet("Sắp hết hàng", ["Sản phẩm", "SKU", "Phiên bản", "Tồn kho"],
                model.LowStock.Select(row => new object?[] { row.ProductName, row.SKU, row.VariantLabel, row.StockQuantity }), 48, 24, 18, 10);

        return new ReportFile(
            $"bao-cao-ban-hang_{model.From:yyyyMMdd}-{model.To:yyyyMMdd}.xlsx",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            workbook.ToBytes());
    }

    private async Task<(ReportViewModel Model, IReadOnlyList<ReportOrderRow> Orders, IReadOnlyList<ReportSoldItemRow> SoldItems)> LoadAsync(
        DateOnly? from,
        DateOnly? to,
        string? preset,
        CancellationToken cancellationToken)
    {
        var (start, end, normalizedPreset) = ResolveRange(from, to, preset);
        var fromUtc = ShopClock.StartOfDayUtc(start);
        var toUtc = ShopClock.StartOfDayUtc(end.AddDays(1));
        var days = end.DayNumber - start.DayNumber + 1;
        var previousFromUtc = ShopClock.StartOfDayUtc(start.AddDays(-days));

        var orders = await _reportRepository.GetOrdersAsync(fromUtc, toUtc, cancellationToken);
        var soldItems = await _reportRepository.GetSoldItemsAsync(fromUtc, toUtc, cancellationToken);
        var previousRevenue = await _reportRepository.GetCompletedRevenueAsync(previousFromUtc, fromUtc, cancellationToken);
        var newCustomers = await _reportRepository.CountNewCustomersAsync(fromUtc, toUtc, cancellationToken);
        var lowStock = await _reportRepository.GetLowStockAsync(LowStockThreshold, cancellationToken);

        bool PlacedInRange(ReportOrderRow order) => order.CreatedAt >= fromUtc && order.CreatedAt < toUtc;
        var placed = orders.Where(PlacedInRange).ToList();
        // Revenue belongs to the period in which the order was delivered, not when it was placed.
        var completed = orders.Where(order => order.CompletedAt >= fromUtc && order.CompletedAt < toUtc).ToList();
        var revenue = completed.Sum(order => order.TotalAmount);

        var groupByMonth = days > MaxDailyPoints;
        var categories = soldItems
            .GroupBy(item => item.CategoryName)
            .Select(group => new { Name = group.Key, Quantity = group.Sum(item => item.Quantity), Revenue = group.Sum(item => item.Revenue) })
            .OrderByDescending(row => row.Revenue)
            .ToList();
        var goodsRevenue = categories.Sum(row => row.Revenue);

        var model = new ReportViewModel
        {
            From = start,
            To = end,
            Preset = normalizedPreset,
            GroupByMonth = groupByMonth,
            Revenue = revenue,
            ShippingRevenue = completed.Sum(order => order.ShippingFee),
            PreviousRevenue = previousRevenue,
            OrdersPlaced = placed.Count,
            OrdersCompleted = completed.Count,
            OrdersCancelled = placed.Count(order => order.Status == OrderStatus.Cancelled),
            ItemsSold = soldItems.Sum(item => item.Quantity),
            NewCustomers = newCustomers,
            Timeline = BuildTimeline(start, end, groupByMonth, placed, completed),
            TopProducts = AggregateProducts(soldItems).Take(TopProductCount).ToList(),
            Categories = categories.Select(row => new ReportCategoryRow(
                row.Name, row.Quantity, row.Revenue,
                goodsRevenue == 0 ? 0 : Math.Round((double)(row.Revenue / goodsRevenue * 100m), 1))).ToList(),
            LowStock = lowStock,
            LowStockThreshold = LowStockThreshold
        };

        return (model, orders, soldItems);
    }

    private static (DateOnly Start, DateOnly End, string? Preset) ResolveRange(DateOnly? from, DateOnly? to, string? preset)
    {
        var today = ShopClock.Today;
        if (from.HasValue || to.HasValue)
        {
            var start = from ?? to!.Value;
            var end = to ?? today;
            if (start > end)
            {
                (start, end) = (end, start);
            }

            if (end.DayNumber - start.DayNumber >= MaxRangeDays)
            {
                start = end.AddDays(-(MaxRangeDays - 1));
            }

            return (start, end, null);
        }

        var monthStart = new DateOnly(today.Year, today.Month, 1);
        return preset switch
        {
            "7d" => (today.AddDays(-6), today, "7d"),
            "this-month" => (monthStart, today, "this-month"),
            "last-month" => (monthStart.AddMonths(-1), monthStart.AddDays(-1), "last-month"),
            "this-year" => (new DateOnly(today.Year, 1, 1), today, "this-year"),
            _ => (today.AddDays(-29), today, "30d")
        };
    }

    private static IReadOnlyList<ReportTimePoint> BuildTimeline(
        DateOnly start,
        DateOnly end,
        bool groupByMonth,
        IReadOnlyList<ReportOrderRow> placed,
        IReadOnlyList<ReportOrderRow> completed)
    {
        DateOnly Bucket(DateTime utc)
        {
            var local = DateOnly.FromDateTime(ShopClock.ToLocal(utc));
            return groupByMonth ? new DateOnly(local.Year, local.Month, 1) : local;
        }

        var placedByBucket = placed.GroupBy(order => Bucket(order.CreatedAt)).ToDictionary(group => group.Key, group => group.Count());
        var completedByBucket = completed.GroupBy(order => Bucket(order.CompletedAt!.Value))
            .ToDictionary(group => group.Key, group => (Count: group.Count(), Revenue: group.Sum(order => order.TotalAmount)));

        var points = new List<ReportTimePoint>();
        var cursor = groupByMonth ? new DateOnly(start.Year, start.Month, 1) : start;
        while (cursor <= end)
        {
            var done = completedByBucket.GetValueOrDefault(cursor);
            points.Add(new ReportTimePoint(
                groupByMonth
                    ? cursor.ToString("MM/yyyy", System.Globalization.CultureInfo.InvariantCulture)
                    : cursor.ToString("dd/MM", System.Globalization.CultureInfo.InvariantCulture),
                cursor,
                done.Revenue,
                placedByBucket.GetValueOrDefault(cursor),
                done.Count));
            cursor = groupByMonth ? cursor.AddMonths(1) : cursor.AddDays(1);
        }

        return points;
    }

    private static IEnumerable<ReportProductRow> AggregateProducts(IEnumerable<ReportSoldItemRow> soldItems) =>
        soldItems
            .GroupBy(item => new { item.ProductId, item.ProductName, item.CategoryName })
            .Select(group => new ReportProductRow(
                group.Key.ProductId, group.Key.ProductName, group.Key.CategoryName,
                group.Sum(item => item.Quantity), group.Sum(item => item.Revenue)))
            .OrderByDescending(row => row.Quantity)
            .ThenByDescending(row => row.Revenue);

    private static string StatusText(OrderStatus status) => status switch
    {
        OrderStatus.Pending => "Chờ xác nhận",
        OrderStatus.Confirmed => "Đã xác nhận",
        OrderStatus.Processing => "Đang xử lý",
        OrderStatus.Shipping => "Đang giao",
        OrderStatus.Completed => "Đã giao thành công",
        OrderStatus.Cancelled => "Đã hủy",
        _ => "Không xác định"
    };
}
