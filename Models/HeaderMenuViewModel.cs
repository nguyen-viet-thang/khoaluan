namespace PopPop.Models;

/// <summary>Catalog links for the storefront header's "Sản phẩm" menu.</summary>
public sealed class HeaderMenu
{
    public IReadOnlyList<HeaderMenuLink> Categories { get; init; } = [];
    public IReadOnlyList<HeaderMenuLink> Brands { get; init; } = [];
}

/// <param name="ProductCount">Products currently on sale in this category/brand.</param>
public sealed record HeaderMenuLink(int Id, string Name, int ProductCount);
