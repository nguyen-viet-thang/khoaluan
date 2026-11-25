namespace PopPop.Data.Entities;

/// <summary>
/// Product entity
/// </summary>
public class Product : IBaseEntity<int>
{
    public int Id { get; set; }

    /// <summary>
    /// Product name
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Product description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Category ID (Foreign Key)
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// Brand ID (Foreign Key)
    /// </summary>
    public int BrandId { get; set; }

    /// <summary>
    /// Is product active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Created timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last updated timestamp
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// A product is for sale only when it, its category and its brand are all active.
    /// Category and Brand must be loaded; database queries use the same rule inline.
    /// </summary>
    public bool IsOnSale() => IsActive && Category?.IsActive == true && Brand?.IsActive == true;

    // Navigation properties
    public virtual Category? Category { get; set; }
    public virtual Brand? Brand { get; set; }
    public virtual ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();
    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
    public virtual ICollection<ProductSpecValue> SpecValues { get; set; } = new List<ProductSpecValue>();
}
