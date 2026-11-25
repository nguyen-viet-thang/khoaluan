namespace PopPop.Data.Entities;

/// <summary>Where a specification value lives.</summary>
public enum SpecScope
{
    /// <summary>Same for the whole product (racket balance, string gauge, shirt gender...).</summary>
    Product = 1,
    /// <summary>Differs per variant and decides which variant is bought (racket weight/grip, size, colour...).</summary>
    Variant = 2
}

/// <summary>
/// A searchable specification defined per category, e.g. "Điểm cân bằng" for rackets or "Size" for shoes.
/// </summary>
public class SpecAttribute
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public required string Name { get; set; }
    public SpecScope Scope { get; set; }
    /// <summary>Short explanation shown to shoppers (e.g. what 3U/4U mean).</summary>
    public string? Hint { get; set; }
    public int SortOrder { get; set; }

    public virtual Category? Category { get; set; }
    public virtual ICollection<SpecOption> Options { get; set; } = new List<SpecOption>();
}

/// <summary>One allowed value of a <see cref="SpecAttribute"/>, e.g. "Nặng đầu" or "4U".</summary>
public class SpecOption
{
    public int Id { get; set; }
    public int SpecAttributeId { get; set; }
    public required string Value { get; set; }
    public string? Hint { get; set; }
    public int SortOrder { get; set; }

    public virtual SpecAttribute? Attribute { get; set; }
}

/// <summary>Product-level specification value (many per product: one or more per attribute).</summary>
public class ProductSpecValue
{
    public int ProductId { get; set; }
    public int SpecOptionId { get; set; }

    public virtual Product? Product { get; set; }
    public virtual SpecOption? Option { get; set; }
}

/// <summary>Variant-level specification value (at most one per attribute for a variant).</summary>
public class VariantSpecValue
{
    public int ProductVariantId { get; set; }
    public int SpecOptionId { get; set; }

    public virtual ProductVariant? Variant { get; set; }
    public virtual SpecOption? Option { get; set; }
}
