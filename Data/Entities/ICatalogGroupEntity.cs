namespace PopPop.Data.Entities;

/// <summary>
/// Common shape of entities that group products (Category, Brand),
/// so they can share one repository/service/controller implementation.
/// </summary>
public interface ICatalogGroupEntity : IBaseEntity<int>
{
    string Name { get; set; }
    string? Description { get; set; }
    bool IsActive { get; set; }
    DateTime CreatedAt { get; set; }
    DateTime UpdatedAt { get; set; }
    ICollection<Product> Products { get; set; }
}
