using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using PopPop.Data.Entities;

namespace PopPop.Data.Seed;

/// <summary>
/// Searchable specifications for the demo catalog.
/// Racket balance/flex follow the manufacturers' classification as listed by authorised dealers
/// (see Docs/Product-Spec-Sources.md); string gauges are the published diameters.
/// Variant-level values (weight, grip, size, colour, speed) are derived from the seeded variant labels.
/// </summary>
internal static class SpecSeedData
{
    private const int Rackets = 1, Shoes = 2, Shirts = 3, Bottoms = 4, Shuttlecocks = 5, Bags = 6, Strings = 7, Grips = 8, Accessories = 9;
    private const SpecScope P = SpecScope.Product, V = SpecScope.Variant;

    private sealed record AttributeSeed(int Id, int CategoryId, string Name, SpecScope Scope, string? Hint, (int Id, string Value, string? Hint)[] Options);

    private static readonly AttributeSeed[] Attributes =
    [
        // ---- Vợt ----
        new(1, Rackets, "Trọng lượng", V, "Ký hiệu U càng lớn vợt càng nhẹ.",
            [(1, "3U", "85–89 g"), (2, "4U", "80–84 g"), (3, "8U", "55–59 g")]),
        new(2, Rackets, "Cỡ cán", V, "Số G càng lớn cán càng nhỏ.",
            [(4, "G5", null), (5, "G6", null)]),
        new(3, Rackets, "Điểm cân bằng", P, "Quyết định cảm giác vung và lối chơi phù hợp.",
            [(6, "Nặng đầu", "Thiên tấn công, đập cầu uy lực"), (7, "Cân bằng", "Công thủ toàn diện"), (8, "Nhẹ đầu", "Phòng thủ, phản tạt nhanh")]),
        new(4, Rackets, "Độ cứng thân", P, "Thân dẻo dễ trợ lực; thân cứng cần lực cổ tay tốt nhưng kiểm soát chính xác hơn.",
            [(9, "Dẻo", "Dễ chơi, hợp người mới"), (10, "Trung bình", "Phù hợp đa số người chơi"), (11, "Cứng", "Cho người chơi có lực cổ tay tốt")]),

        // ---- Giày ----
        new(5, Shoes, "Size", V, null,
            [(12, "38", null), (13, "39", null), (14, "40", null), (15, "41", null), (16, "42", null), (17, "43", null)]),
        new(6, Shoes, "Màu", P, null, [(18, "Trắng", null), (19, "Đen", null)]),

        // ---- Áo ----
        new(7, Shirts, "Dành cho", P, null, [(20, "Nam", null), (21, "Nữ", null)]),
        new(8, Shirts, "Size", V, null, [(22, "S", null), (23, "M", null), (24, "L", null), (25, "XL", null)]),
        new(9, Shirts, "Màu", P, null,
            [(26, "Trắng", null), (27, "Đen", null), (28, "Đỏ", null), (29, "Xanh navy", null), (30, "Xanh dương", null)]),

        // ---- Quần & váy ----
        new(10, Bottoms, "Loại", P, null, [(31, "Quần short", null), (32, "Váy", null)]),
        new(11, Bottoms, "Dành cho", P, null, [(33, "Nam", null), (34, "Nữ", null)]),
        new(12, Bottoms, "Size", V, null, [(35, "S", null), (36, "M", null), (37, "L", null), (38, "XL", null)]),
        new(13, Bottoms, "Màu", P, null, [(39, "Đen", null), (40, "Xanh navy", null)]),

        // ---- Quả cầu ----
        new(14, Shuttlecocks, "Tốc độ", V, "Chọn theo nhiệt độ và độ cao của sân: sân nóng chọn số nhỏ, sân lạnh chọn số lớn.",
            [(41, "76", null), (42, "77", null)]),

        // ---- Túi & balo ----
        new(15, Bags, "Loại", P, null, [(43, "Túi vợt", null), (44, "Balo", null)]),
        new(16, Bags, "Màu", V, null, [(45, "Nâu", null), (46, "Đen", null), (47, "Hồng", null), (48, "Xanh navy", null)]),

        // ---- Cước ----
        new(17, Strings, "Đường kính", P, "Cước mảnh nảy hơn, đanh tiếng; cước dày bền hơn.",
            [(49, "0,63 mm", null), (50, "0,65 mm", null), (51, "0,66 mm", null), (52, "0,68 mm", null), (53, "0,70 mm", null)]),
        new(18, Strings, "Màu", V, null, [(54, "Trắng", null), (55, "Vàng", null)]),

        // ---- Quấn cán ----
        new(19, Grips, "Loại", P, null, [(56, "Cao su (PU)", "Bám tay, mỏng"), (57, "Khăn", "Thấm mồ hôi tốt")]),
        new(20, Grips, "Màu", V, null, [(58, "Trắng", null), (59, "Đen", null), (60, "Vàng", null), (61, "Đỏ", null)]),

        // ---- Phụ kiện ----
        new(21, Accessories, "Loại", P, null, [(62, "Tất", null), (63, "Băng cổ tay", null), (64, "Lưới", null)])
    ];

    /// <summary>Product-level values by product id (option ids from <see cref="Attributes"/>).</summary>
    private static readonly Dictionary<int, int[]> ProductValues = new()
    {
        // Rackets: balance, flex
        [1] = [6, 11],   // Astrox 99 Pro: head heavy, stiff
        [2] = [6, 11],   // Astrox 88D Pro: head heavy, stiff
        [3] = [8, 11],   // Nanoflare 800 Pro: head light, stiff
        [4] = [7, 11],   // Arcsaber 11 Pro: even, stiff
        [5] = [8, 11],   // Nanoflare 1000Z: head light, stiff
        [6] = [6, 10],   // Astrox 77 Play: head heavy, medium
        [7] = [8, 10],   // Nanoflare 800 Play: head light, medium
        [8] = [6, 11],   // Thruster Ryuga II Pro: head heavy, stiff
        [9] = [8, 11],   // Auraspeed 90K II: head light, stiff
        [10] = [7, 10],  // Brave Sword 12: even, medium
        [11] = [6, 10],  // Axforce 80: head heavy, medium
        [12] = [6, 10],  // Aeronaut 9000C: head heavy, medium
        [13] = [7, 11],  // Halbertec 8000: even (all-round), stiff
        [14] = [6, 9],   // Feather Weight 55: head heavy, flexible
        // Shoes: colour
        [15] = [18], [16] = [18], [17] = [19], [18] = [19], [19] = [18],
        // Shirts: gender, colour
        [20] = [20, 29], [21] = [21, 26], [22] = [20, 28], [23] = [20, 27], [24] = [20, 30],
        // Bottoms: type, gender, colour
        [25] = [31, 33, 39], [26] = [32, 34, 40], [27] = [31, 33, 39],
        // Bags: type
        [32] = [43], [33] = [44], [34] = [43],
        // Strings: gauge
        [35] = [53], [36] = [50], [37] = [52], [38] = [49], [39] = [51], [40] = [50],
        // Grips: type
        [41] = [56], [42] = [56], [43] = [57],
        // Accessories: type
        [44] = [62], [45] = [63], [46] = [64]
    };

    public static void Apply(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SpecAttribute>().HasData(Attributes.Select((attribute, index) => new SpecAttribute
        {
            Id = attribute.Id,
            CategoryId = attribute.CategoryId,
            Name = attribute.Name,
            Scope = attribute.Scope,
            Hint = attribute.Hint,
            SortOrder = index + 1
        }));

        modelBuilder.Entity<SpecOption>().HasData(Attributes.SelectMany(attribute => attribute.Options.Select((option, index) => new SpecOption
        {
            Id = option.Id,
            SpecAttributeId = attribute.Id,
            Value = option.Value,
            Hint = option.Hint,
            SortOrder = index + 1
        })));

        modelBuilder.Entity<ProductSpecValue>().HasData(ProductValues.SelectMany(pair =>
            pair.Value.Select(optionId => new ProductSpecValue { ProductId = pair.Key, SpecOptionId = optionId })));

        modelBuilder.Entity<VariantSpecValue>().HasData(BuildVariantValues());
    }

    /// <summary>Matches each variant label ("3U - G5", "Size 41", "Màu xanh navy", "Tốc độ 2 (76)") to its options.</summary>
    private static List<VariantSpecValue> BuildVariantValues()
    {
        var result = new List<VariantSpecValue>();
        foreach (var (variantId, _, categoryId, label) in CatalogSeedData.SeedVariants())
        {
            foreach (var attribute in Attributes.Where(item => item.CategoryId == categoryId && item.Scope == V))
            {
                // Longest value first so "Xanh navy" wins over a shorter value contained in it.
                var match = attribute.Options
                    .OrderByDescending(option => option.Value.Length)
                    .FirstOrDefault(option => Regex.IsMatch(label,
                        $@"(?<![\p{{L}}\d]){Regex.Escape(option.Value)}(?![\p{{L}}\d])", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant));
                if (match.Value is null)
                {
                    throw new InvalidOperationException($"Seed variant {variantId} ('{label}') has no value for '{attribute.Name}'.");
                }

                result.Add(new VariantSpecValue { ProductVariantId = variantId, SpecOptionId = match.Id });
            }
        }

        return result;
    }
}
