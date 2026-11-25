namespace PopPop.Models;

public sealed class ProductCategoryOptionViewModel
{
    public int Id { get; init; }
    public required string Name { get; init; }
}

public sealed class ProductCatalogViewModel
{
    public IReadOnlyList<ProductCardViewModel> Products { get; init; } = [];
    public IReadOnlyList<ProductCategoryOptionViewModel> Categories { get; init; } = [];
    /// <summary>Active brands (same Id/Name shape as categories).</summary>
    public IReadOnlyList<ProductCategoryOptionViewModel> Brands { get; init; } = [];
    public int? CategoryId { get; init; }
    public int? BrandId { get; init; }
    public string? Search { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public decimal PriceRangeMax { get; init; }
    public string SortBy { get; init; } = "newest";
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    /// <summary>Specification filters of the selected category (empty when no category is selected).</summary>
    public IReadOnlyList<SpecFilterGroupViewModel> SpecFilters { get; init; } = [];
    /// <summary>Valid selected option ids (invalid/foreign ids are dropped).</summary>
    public IReadOnlyList<int> SelectedSpecIds { get; init; } = [];

    /// <summary>Current query; callers tweak a copy to build links.</summary>
    public CatalogQuery Query(Action<CatalogQuery>? change = null)
    {
        var query = new CatalogQuery
        {
            CategoryId = CategoryId,
            BrandId = BrandId,
            Q = Search,
            MinPrice = MinPrice,
            MaxPrice = MaxPrice,
            Sort = SortBy,
            Spec = SelectedSpecIds.ToList(),
            Page = 1
        };
        change?.Invoke(query);
        return query;
    }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
