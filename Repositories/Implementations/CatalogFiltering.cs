using PopPop.Data;
using PopPop.Data.Entities;
using PopPop.Models;

namespace PopPop.Repositories.Implementations;

/// <summary>
/// Shop-catalog filtering shared by the product list, the header search and the facet counts,
/// so all three always agree on which products match.
/// </summary>
internal static class CatalogFiltering
{
    private const int MaxSearchTokens = 6;

    /// <summary>Products a customer can see: product, category and brand active, with at least one variant in stock.</summary>
    public static IQueryable<Product> VisibleProducts(PopPopDbContext context) =>
        context.Products.Where(product => product.IsActive && product.Category!.IsActive && product.Brand!.IsActive
            && product.ProductVariants.Any(variant => variant.IsActive && variant.StockQuantity > 0));

    /// <summary>Buyable variants that satisfy every variant-level group (one variant must match all, e.g. 4U AND G6).</summary>
    public static IQueryable<ProductVariant> MatchingVariants(PopPopDbContext context, IEnumerable<SpecFilterGroup> variantGroups)
    {
        var variants = context.ProductVariants.Where(variant => variant.IsActive && variant.StockQuantity > 0);
        foreach (var group in variantGroups)
        {
            var optionIds = group.OptionIds;
            variants = variants.Where(variant => variant.SpecValues.Any(value => optionIds.Contains(value.SpecOptionId)));
        }

        return variants;
    }

    public static IQueryable<Product> ApplyFilter(this IQueryable<Product> query, PopPopDbContext context, CatalogFilter filter)
    {
        if (filter.CategoryId.HasValue)
        {
            var categoryId = filter.CategoryId.Value;
            query = query.Where(product => product.CategoryId == categoryId);
        }

        if (filter.BrandId.HasValue)
        {
            var brandId = filter.BrandId.Value;
            query = query.Where(product => product.BrandId == brandId);
        }

        // Every word must appear in the name, brand, category or one of the product's specifications,
        // so "vợt yonex nhẹ đầu" or "giày 42" work without opening the filters.
        foreach (var token in Tokenize(filter.Search))
        {
            query = query.Where(product => product.Name.Contains(token)
                || product.Brand!.Name.Contains(token)
                || product.Category!.Name.Contains(token)
                || product.SpecValues.Any(value => value.Option!.Value.Contains(token))
                || product.ProductVariants.Any(variant => variant.IsActive && variant.StockQuantity > 0
                    && variant.SpecValues.Any(value => value.Option!.Value.Contains(token))));
        }

        foreach (var group in filter.Specs.Where(group => group.Scope == SpecScope.Product))
        {
            var optionIds = group.OptionIds;
            query = query.Where(product => product.SpecValues.Any(value => optionIds.Contains(value.SpecOptionId)));
        }

        var variantGroups = filter.Specs.Where(group => group.Scope == SpecScope.Variant).ToList();
        if (variantGroups.Count > 0)
        {
            var matching = MatchingVariants(context, variantGroups);
            query = query.Where(product => matching.Any(variant => variant.ProductId == product.Id));
        }

        if (filter.MinPrice.HasValue)
        {
            var minPrice = filter.MinPrice.Value;
            query = query.Where(product => product.ProductVariants
                .Where(variant => variant.IsActive && variant.StockQuantity > 0)
                .Min(variant => (decimal?)variant.Price) >= minPrice);
        }

        if (filter.MaxPrice.HasValue)
        {
            var maxPrice = filter.MaxPrice.Value;
            query = query.Where(product => product.ProductVariants
                .Where(variant => variant.IsActive && variant.StockQuantity > 0)
                .Min(variant => (decimal?)variant.Price) <= maxPrice);
        }

        return query;
    }

    private static IEnumerable<string> Tokenize(string? search) =>
        string.IsNullOrWhiteSpace(search)
            ? []
            : search.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(MaxSearchTokens);
}
