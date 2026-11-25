using PopPop.Data.Entities;
using PopPop.Models;

namespace PopPop.Repositories.Interfaces;

/// <summary>Product specifications: definitions per category, values per product/variant, and facet counts.</summary>
public interface ISpecRepository
{
    /// <summary>Attributes of a category with their options, ordered (read-only).</summary>
    Task<IReadOnlyList<SpecAttribute>> GetAttributesAsync(int categoryId, CancellationToken cancellationToken = default);

    /// <summary>Options by id with their attribute (read-only); unknown ids are skipped.</summary>
    Task<IReadOnlyList<SpecOption>> GetOptionsAsync(IReadOnlyCollection<int> optionIds, CancellationToken cancellationToken = default);

    /// <summary>For each option of the attribute: number of shop-visible products matching <paramref name="filter"/> that have it.</summary>
    Task<Dictionary<int, int>> CountProductsByOptionAsync(
        CatalogFilter filter,
        SpecAttribute attribute,
        CancellationToken cancellationToken = default);

    /// <summary>Product-level and variant-level values of a product (read-only, with option and attribute).</summary>
    Task<(IReadOnlyList<ProductSpecValue> ProductValues, IReadOnlyList<VariantSpecValue> VariantValues)> GetValuesForProductAsync(
        int productId,
        CancellationToken cancellationToken = default);

    /// <summary>Replaces the product-level values (tracked; saved by the unit of work).</summary>
    Task ReplaceProductValuesAsync(int productId, IReadOnlyCollection<int> optionIds, CancellationToken cancellationToken = default);

    /// <summary>Replaces the values of a saved variant (tracked; saved by the unit of work).</summary>
    Task ReplaceVariantValuesAsync(int variantId, IReadOnlyCollection<int> optionIds, CancellationToken cancellationToken = default);

    /// <summary>Removes product and variant values that belong to another category (after a category change).</summary>
    Task RemoveValuesOutsideCategoryAsync(int productId, int categoryId, CancellationToken cancellationToken = default);

    // ---- Definition management (admin) ----

    /// <summary>Tracked attributes of a category with their options (for reordering / editing).</summary>
    Task<List<SpecAttribute>> GetAttributesTrackedAsync(int categoryId, CancellationToken cancellationToken = default);

    /// <summary>Tracked attribute with its options.</summary>
    Task<SpecAttribute?> GetAttributeTrackedAsync(int attributeId, CancellationToken cancellationToken = default);

    /// <summary>Tracked option with its attribute and the attribute's options.</summary>
    Task<SpecOption?> GetOptionTrackedAsync(int optionId, CancellationToken cancellationToken = default);

    Task<bool> AttributeNameExistsAsync(int categoryId, string name, int? excludeAttributeId, CancellationToken cancellationToken = default);
    Task<bool> OptionValueExistsAsync(int attributeId, string value, int? excludeOptionId, CancellationToken cancellationToken = default);

    /// <summary>For each option of the category: number of products (product-level) or variants (variant-level) using it.</summary>
    Task<Dictionary<int, int>> CountOptionUsageAsync(int categoryId, CancellationToken cancellationToken = default);

    /// <summary>Names of up to <paramref name="take"/> products that use any of the options.</summary>
    Task<IReadOnlyList<string>> GetProductNamesUsingAsync(IReadOnlyCollection<int> optionIds, int take, CancellationToken cancellationToken = default);

    Task AddAttributeAsync(SpecAttribute attribute, CancellationToken cancellationToken = default);
    Task AddOptionAsync(SpecOption option, CancellationToken cancellationToken = default);
    void RemoveAttribute(SpecAttribute attribute);
    void RemoveOption(SpecOption option);
}
