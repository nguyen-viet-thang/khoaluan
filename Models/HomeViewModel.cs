namespace PopPop.Models;

/// <summary>Category or brand with the number of products currently for sale and a representative image.</summary>
public sealed record CatalogGroupSummary(int Id, string Name, string? Description, int ProductCount, string? ImageUrl);

public sealed class HomeViewModel
{
    /// <summary>Products shown in the hero slider.</summary>
    public IReadOnlyList<HomeFeaturedProductViewModel> Featured { get; init; } = [];
    public IReadOnlyList<ProductCardViewModel> LatestProducts { get; init; } = [];
    /// <summary>Best sellers from real (non-cancelled) orders; empty while the shop has no sales.</summary>
    public IReadOnlyList<ProductCardViewModel> BestSellers { get; init; } = [];
    public IReadOnlyList<CatalogGroupSummary> Categories { get; init; } = [];
    public IReadOnlyList<CatalogGroupSummary> Brands { get; init; } = [];
}

public sealed class HomeFeaturedProductViewModel
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public string? ImageUrl { get; init; }
    public string? CategoryName { get; init; }
    public decimal Price { get; init; }
}
