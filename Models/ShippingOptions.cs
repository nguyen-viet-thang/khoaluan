namespace PopPop.Models;

/// <summary>
/// Shipping fee policy, bound from the "Shipping" section of appsettings.json.
/// </summary>
public sealed class ShippingOptions
{
    public const string SectionName = "Shipping";

    /// <summary>Flat delivery fee in VND.</summary>
    public decimal Fee { get; set; } = 30_000m;

    /// <summary>Orders whose subtotal reaches this amount ship for free. 0 disables free shipping.</summary>
    public decimal FreeShippingThreshold { get; set; } = 500_000m;

    public decimal CalculateFee(decimal subtotal) =>
        subtotal <= 0 || (FreeShippingThreshold > 0 && subtotal >= FreeShippingThreshold) ? 0m : Fee;
}
