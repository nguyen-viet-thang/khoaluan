using Microsoft.EntityFrameworkCore;
using PopPop.Data;
using PopPop.Data.Entities;
using PopPop.Models;
using PopPop.Repositories.Interfaces;

namespace PopPop.Repositories.Implementations;

public sealed class SpecRepository : ISpecRepository
{
    private readonly PopPopDbContext _context;

    public SpecRepository(PopPopDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<SpecAttribute>> GetAttributesAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        return await _context.SpecAttributes
            .AsNoTracking()
            .Where(attribute => attribute.CategoryId == categoryId)
            .Include(attribute => attribute.Options.OrderBy(option => option.SortOrder).ThenBy(option => option.Id))
            .OrderBy(attribute => attribute.SortOrder)
            .ThenBy(attribute => attribute.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SpecOption>> GetOptionsAsync(IReadOnlyCollection<int> optionIds, CancellationToken cancellationToken = default)
    {
        if (optionIds.Count == 0)
        {
            return [];
        }

        return await _context.SpecOptions
            .AsNoTracking()
            .Include(option => option.Attribute)
            .Where(option => optionIds.Contains(option.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<int, int>> CountProductsByOptionAsync(
        CatalogFilter filter,
        SpecAttribute attribute,
        CancellationToken cancellationToken = default)
    {
        var attributeId = attribute.Id;
        var productIds = CatalogFiltering.VisibleProducts(_context).ApplyFilter(_context, filter).Select(product => product.Id);

        if (attribute.Scope == SpecScope.Product)
        {
            return await _context.ProductSpecValues
                .Where(value => value.Option!.SpecAttributeId == attributeId && productIds.Contains(value.ProductId))
                .GroupBy(value => value.SpecOptionId)
                .Select(group => new { group.Key, Count = group.Count() })
                .ToDictionaryAsync(row => row.Key, row => row.Count, cancellationToken);
        }

        // Variant level: the same buyable variant must also satisfy the other variant filters (e.g. 4U together with G6).
        var variants = CatalogFiltering
            .MatchingVariants(_context, filter.Specs.Where(group => group.Scope == SpecScope.Variant))
            .Where(variant => productIds.Contains(variant.ProductId));

        return await _context.VariantSpecValues
            .Where(value => value.Option!.SpecAttributeId == attributeId
                && variants.Any(variant => variant.Id == value.ProductVariantId))
            .GroupBy(value => value.SpecOptionId)
            .Select(group => new { group.Key, Count = group.Select(value => value.Variant!.ProductId).Distinct().Count() })
            .ToDictionaryAsync(row => row.Key, row => row.Count, cancellationToken);
    }

    public async Task<(IReadOnlyList<ProductSpecValue> ProductValues, IReadOnlyList<VariantSpecValue> VariantValues)> GetValuesForProductAsync(
        int productId,
        CancellationToken cancellationToken = default)
    {
        var productValues = await _context.ProductSpecValues
            .AsNoTracking()
            .Include(value => value.Option!).ThenInclude(option => option.Attribute)
            .Where(value => value.ProductId == productId)
            .ToListAsync(cancellationToken);

        var variantValues = await _context.VariantSpecValues
            .AsNoTracking()
            .Include(value => value.Option!).ThenInclude(option => option.Attribute)
            .Include(value => value.Variant)
            .Where(value => value.Variant!.ProductId == productId)
            .ToListAsync(cancellationToken);

        return (productValues, variantValues);
    }

    public async Task ReplaceProductValuesAsync(int productId, IReadOnlyCollection<int> optionIds, CancellationToken cancellationToken = default)
    {
        var existing = await _context.ProductSpecValues.Where(value => value.ProductId == productId).ToListAsync(cancellationToken);
        _context.ProductSpecValues.RemoveRange(existing.Where(value => !optionIds.Contains(value.SpecOptionId)));
        var existingIds = existing.Select(value => value.SpecOptionId).ToHashSet();
        await _context.ProductSpecValues.AddRangeAsync(
            optionIds.Where(id => !existingIds.Contains(id)).Select(id => new ProductSpecValue { ProductId = productId, SpecOptionId = id }),
            cancellationToken);
    }

    public async Task ReplaceVariantValuesAsync(int variantId, IReadOnlyCollection<int> optionIds, CancellationToken cancellationToken = default)
    {
        var existing = await _context.VariantSpecValues.Where(value => value.ProductVariantId == variantId).ToListAsync(cancellationToken);
        _context.VariantSpecValues.RemoveRange(existing.Where(value => !optionIds.Contains(value.SpecOptionId)));
        var existingIds = existing.Select(value => value.SpecOptionId).ToHashSet();
        await _context.VariantSpecValues.AddRangeAsync(
            optionIds.Where(id => !existingIds.Contains(id)).Select(id => new VariantSpecValue { ProductVariantId = variantId, SpecOptionId = id }),
            cancellationToken);
    }

    public async Task RemoveValuesOutsideCategoryAsync(int productId, int categoryId, CancellationToken cancellationToken = default)
    {
        var productValues = await _context.ProductSpecValues
            .Where(value => value.ProductId == productId && value.Option!.Attribute!.CategoryId != categoryId)
            .ToListAsync(cancellationToken);
        var variantValues = await _context.VariantSpecValues
            .Where(value => value.Variant!.ProductId == productId && value.Option!.Attribute!.CategoryId != categoryId)
            .ToListAsync(cancellationToken);
        _context.ProductSpecValues.RemoveRange(productValues);
        _context.VariantSpecValues.RemoveRange(variantValues);
    }

    public async Task<List<SpecAttribute>> GetAttributesTrackedAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        return await _context.SpecAttributes
            .Include(attribute => attribute.Options)
            .Where(attribute => attribute.CategoryId == categoryId)
            .OrderBy(attribute => attribute.SortOrder)
            .ThenBy(attribute => attribute.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<SpecAttribute?> GetAttributeTrackedAsync(int attributeId, CancellationToken cancellationToken = default)
    {
        return await _context.SpecAttributes
            .Include(attribute => attribute.Options)
            .FirstOrDefaultAsync(attribute => attribute.Id == attributeId, cancellationToken);
    }

    public async Task<SpecOption?> GetOptionTrackedAsync(int optionId, CancellationToken cancellationToken = default)
    {
        return await _context.SpecOptions
            .Include(option => option.Attribute!).ThenInclude(attribute => attribute.Options)
            .FirstOrDefaultAsync(option => option.Id == optionId, cancellationToken);
    }

    public Task<bool> AttributeNameExistsAsync(int categoryId, string name, int? excludeAttributeId, CancellationToken cancellationToken = default) =>
        _context.SpecAttributes.AnyAsync(attribute => attribute.CategoryId == categoryId && attribute.Name == name
            && (!excludeAttributeId.HasValue || attribute.Id != excludeAttributeId.Value), cancellationToken);

    public Task<bool> OptionValueExistsAsync(int attributeId, string value, int? excludeOptionId, CancellationToken cancellationToken = default) =>
        _context.SpecOptions.AnyAsync(option => option.SpecAttributeId == attributeId && option.Value == value
            && (!excludeOptionId.HasValue || option.Id != excludeOptionId.Value), cancellationToken);

    public async Task<Dictionary<int, int>> CountOptionUsageAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        var productUsage = await _context.ProductSpecValues
            .Where(value => value.Option!.Attribute!.CategoryId == categoryId)
            .GroupBy(value => value.SpecOptionId)
            .Select(group => new { group.Key, Count = group.Count() })
            .ToListAsync(cancellationToken);
        var variantUsage = await _context.VariantSpecValues
            .Where(value => value.Option!.Attribute!.CategoryId == categoryId)
            .GroupBy(value => value.SpecOptionId)
            .Select(group => new { group.Key, Count = group.Count() })
            .ToListAsync(cancellationToken);

        return productUsage.Concat(variantUsage)
            .GroupBy(row => row.Key)
            .ToDictionary(group => group.Key, group => group.Sum(row => row.Count));
    }

    public async Task<IReadOnlyList<string>> GetProductNamesUsingAsync(IReadOnlyCollection<int> optionIds, int take, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .AsNoTracking()
            .Where(product => product.SpecValues.Any(value => optionIds.Contains(value.SpecOptionId))
                || product.ProductVariants.Any(variant => variant.SpecValues.Any(value => optionIds.Contains(value.SpecOptionId))))
            .OrderBy(product => product.Name)
            .Select(product => product.Name)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAttributeAsync(SpecAttribute attribute, CancellationToken cancellationToken = default) =>
        await _context.SpecAttributes.AddAsync(attribute, cancellationToken);

    public async Task AddOptionAsync(SpecOption option, CancellationToken cancellationToken = default) =>
        await _context.SpecOptions.AddAsync(option, cancellationToken);

    public void RemoveAttribute(SpecAttribute attribute) => _context.SpecAttributes.Remove(attribute);

    public void RemoveOption(SpecOption option) => _context.SpecOptions.Remove(option);
}
