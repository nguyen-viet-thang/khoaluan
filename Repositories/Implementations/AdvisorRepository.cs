using Microsoft.EntityFrameworkCore;
using PopPop.Data;
using PopPop.Data.Entities;
using PopPop.Models;
using PopPop.Repositories.Interfaces;

namespace PopPop.Repositories.Implementations;

public sealed class AdvisorRepository(PopPopDbContext context) : IAdvisorRepository
{
    // Specification names as created in Data/Seed/SpecSeedData (admins can add options, not rename these).
    internal const string BalanceAttribute = "Điểm cân bằng";
    internal const string StiffnessAttribute = "Độ cứng thân";
    internal const string WeightAttribute = "Trọng lượng";
    internal const string GaugeAttribute = "Đường kính";

    public async Task<IReadOnlyList<RacketCandidate>> GetRacketsAsync(CancellationToken cancellationToken = default)
    {
        var categoryId = await CategoryOfAttributeAsync(BalanceAttribute, cancellationToken);
        if (categoryId is null)
        {
            return [];
        }

        var products = await OnSale(categoryId.Value)
            .Include(product => product.Brand)
            .Include(product => product.ProductImages)
            .Include(product => product.SpecValues).ThenInclude(value => value.Option!).ThenInclude(option => option.Attribute)
            .Include(product => product.ProductVariants.Where(variant => variant.IsActive))
                .ThenInclude(variant => variant.SpecValues).ThenInclude(value => value.Option!).ThenInclude(option => option.Attribute)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

        return products.Select(product => new RacketCandidate(
            product.Id,
            product.Name,
            product.BrandId,
            product.Brand?.Name ?? string.Empty,
            PrimaryImage(product),
            Choice(product.SpecValues.Select(value => value.Option), BalanceAttribute),
            Choice(product.SpecValues.Select(value => value.Option), StiffnessAttribute),
            product.ProductVariants
                .Select(variant => new RacketVariantCandidate(
                    variant.Id,
                    variant.VariantLabel,
                    variant.Price,
                    variant.StockQuantity,
                    Choice(variant.SpecValues.Select(value => value.Option), WeightAttribute)))
                .ToList())).ToList();
    }

    public async Task<IReadOnlyList<StringCandidate>> GetStringsAsync(CancellationToken cancellationToken = default)
    {
        var categoryId = await CategoryOfAttributeAsync(GaugeAttribute, cancellationToken);
        if (categoryId is null)
        {
            return [];
        }

        var products = await OnSale(categoryId.Value)
            .Include(product => product.ProductImages)
            .Include(product => product.SpecValues).ThenInclude(value => value.Option!).ThenInclude(option => option.Attribute)
            .Include(product => product.ProductVariants.Where(variant => variant.IsActive && variant.StockQuantity > 0))
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

        return products
            .Select(product => (product, gauge: Choice(product.SpecValues.Select(value => value.Option), GaugeAttribute),
                variant: product.ProductVariants.OrderBy(variant => variant.Price).FirstOrDefault()))
            .Where(item => item.gauge is not null && item.variant is not null)
            .Select(item => new StringCandidate(item.product.Id, item.product.Name, PrimaryImage(item.product),
                item.variant!.Id, item.variant.Price, item.gauge!.Value))
            .ToList();
    }

    public async Task<IReadOnlyList<HeaderMenuLink>> GetRacketBrandsAsync(CancellationToken cancellationToken = default)
    {
        var categoryId = await CategoryOfAttributeAsync(BalanceAttribute, cancellationToken);
        if (categoryId is null)
        {
            return [];
        }

        var brands = await OnSale(categoryId.Value)
            .GroupBy(product => new { product.BrandId, product.Brand!.Name })
            .Select(group => new HeaderMenuLink(group.Key.BrandId, group.Key.Name, group.Count()))
            .ToListAsync(cancellationToken);
        return brands.OrderByDescending(link => link.ProductCount).ThenBy(link => link.Name).ToList();
    }

    private Task<int?> CategoryOfAttributeAsync(string attributeName, CancellationToken cancellationToken) =>
        context.SpecAttributes.AsNoTracking()
            .Where(attribute => attribute.Name == attributeName)
            .Select(attribute => (int?)attribute.CategoryId)
            .FirstOrDefaultAsync(cancellationToken);

    private IQueryable<Product> OnSale(int categoryId) =>
        context.Products.AsNoTracking()
            .Where(product => product.CategoryId == categoryId
                && product.IsActive && product.Category!.IsActive && product.Brand!.IsActive);

    private static string? PrimaryImage(Product product) =>
        product.ProductImages.OrderByDescending(image => image.IsPrimary).ThenBy(image => image.SortOrder)
            .Select(image => image.ImageUrl).FirstOrDefault();

    private static SpecChoice? Choice(IEnumerable<SpecOption?> options, string attributeName) =>
        options.Where(option => option?.Attribute?.Name == attributeName)
            .Select(option => new SpecChoice(option!.Value, option.Hint))
            .FirstOrDefault();
}
