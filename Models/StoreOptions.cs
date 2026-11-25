namespace PopPop.Models;

/// <summary>
/// Public shop information shown in the storefront footer, bound from the "Store" section of appsettings.json.
/// Empty values are simply not displayed.
/// </summary>
public sealed class StoreOptions
{
    public const string SectionName = "Store";

    public string Name { get; set; } = "PôpPôp";
    public string? Hotline { get; set; }
    public string? Address { get; set; }
    public string? OpeningHours { get; set; }
    public string? FacebookUrl { get; set; }
}
