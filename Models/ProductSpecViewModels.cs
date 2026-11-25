using Microsoft.AspNetCore.WebUtilities;
using PopPop.Data.Entities;

namespace PopPop.Models;

/// <summary>Selected options of one attribute: a product matches when it has ANY of them (OR inside, AND across attributes).</summary>
public sealed record SpecFilterGroup(int AttributeId, SpecScope Scope, IReadOnlyList<int> OptionIds);

/// <summary>Everything the shop catalog can be filtered by.</summary>
public sealed record CatalogFilter(
    int? CategoryId,
    int? BrandId,
    string? Search,
    decimal? MinPrice,
    decimal? MaxPrice,
    IReadOnlyList<SpecFilterGroup> Specs)
{
    public static readonly CatalogFilter Empty = new(null, null, null, null, null, []);

    public CatalogFilter WithoutAttribute(int attributeId) =>
        this with { Specs = Specs.Where(group => group.AttributeId != attributeId).ToList() };
}

public sealed class SpecFilterOptionViewModel
{
    public int Id { get; init; }
    public string Value { get; init; } = string.Empty;
    public string? Hint { get; init; }
    /// <summary>Products that would match if this option were (also) selected, given the other filters.</summary>
    public int ProductCount { get; init; }
    public bool Selected { get; init; }
}

public sealed class SpecFilterGroupViewModel
{
    public int AttributeId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Hint { get; init; }
    public bool IsVariantLevel { get; init; }
    public IReadOnlyList<SpecFilterOptionViewModel> Options { get; init; } = [];
}

/// <summary>Spec line on the product page, e.g. "Điểm cân bằng: Nặng đầu" or "Trọng lượng: 3U, 4U".</summary>
public sealed record ProductSpecLineViewModel(string Name, string? Hint, IReadOnlyList<ProductSpecValueViewModel> Values);

/// <summary>A value that links to the catalog filtered by it.</summary>
public sealed record ProductSpecValueViewModel(int OptionId, string Value, string? Hint);

/// <summary>Filters shown in the header search panel for a category.</summary>
public sealed class SearchFiltersViewModel
{
    public int? CategoryId { get; init; }
    public IReadOnlyList<ProductCategoryOptionViewModel> Categories { get; init; } = [];
    public IReadOnlyList<ProductCategoryOptionViewModel> Brands { get; init; } = [];
    public IReadOnlyList<SpecFilterGroupViewModel> Specs { get; init; } = [];
}

/// <summary>Live search results in the header panel.</summary>
public sealed class SearchResultsViewModel
{
    public IReadOnlyList<ProductCardViewModel> Products { get; init; } = [];
    public int TotalCount { get; init; }
    /// <summary>Catalog URL with the same filters ("Xem tất cả").</summary>
    public string CatalogUrl { get; init; } = "/Products";
}

/// <summary>Mutable copy of the catalog query string, used to build filter/paging links.</summary>
public sealed class CatalogQuery
{
    public int? CategoryId { get; set; }
    public int? BrandId { get; set; }
    public string? Q { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? Sort { get; set; }
    public List<int> Spec { get; set; } = [];
    public int Page { get; set; } = 1;

    public string ToUrl(string basePath)
    {
        var pairs = new List<KeyValuePair<string, string?>>();
        void Add(string key, object? value)
        {
            if (value is not null && !(value is string text && string.IsNullOrWhiteSpace(text)))
            {
                pairs.Add(new(key, Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture)));
            }
        }

        Add("q", Q);
        Add("categoryId", CategoryId);
        Add("brandId", BrandId);
        foreach (var id in Spec.Distinct())
        {
            Add("spec", id);
        }

        Add("minPrice", MinPrice);
        Add("maxPrice", MaxPrice);
        Add("sort", Sort is null or "newest" ? null : Sort);
        Add("page", Page > 1 ? Page : null);
        return QueryHelpers.AddQueryString(basePath, pairs);
    }
}
