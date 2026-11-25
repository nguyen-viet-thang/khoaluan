using Microsoft.EntityFrameworkCore;
using PopPop.Data.Entities;
using PopPop.Data.Entities.Enums;

namespace PopPop.Data.Seed;

/// <summary>
/// Demo catalog for a Vietnamese badminton shop: real product models from real brands,
/// variants that match how each product type is actually sold (racket weight/grip, shoe size,
/// clothing size, shuttle speed, colour), and reference prices in VND.
/// Prices are approximate market prices and only meant for demonstration.
/// Product photos are manufacturer/dealer images stored in wwwroot/img/product/catalog;
/// their sources are listed in Docs/Product-Image-Sources.md.
/// </summary>
internal static class CatalogSeedData
{
    private static readonly DateTime SeedDate = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private const int Rackets = 1, Shoes = 2, Shirts = 3, Bottoms = 4, Shuttlecocks = 5, Bags = 6, Strings = 7, Grips = 8, Accessories = 9;
    private const int Yonex = 1, Victor = 2, LiNing = 3, Apacs = 4;

    private sealed record VariantSeed(string Code, string Label, decimal Price, int Stock);

    private sealed record ProductSeed(
        int Id,
        string Name,
        int CategoryId,
        int BrandId,
        string SkuPrefix,
        string Image,
        string Description,
        VariantSeed[] Variants,
        int DaysSinceLaunch);

    private static readonly (int Id, string Name, string Description)[] CategorySeeds =
    [
        (Rackets, "Vợt cầu lông", "Vợt chính hãng từ vợt tập luyện cho người mới đến vợt thi đấu chuyên nghiệp. Phân loại theo trọng lượng (3U: 85-89 g, 4U: 80-84 g) và cỡ cán (G5, G6)."),
        (Shoes, "Giày cầu lông", "Giày chuyên dụng đế cao su bám sân, đệm giảm chấn, hỗ trợ di chuyển ngang và bảo vệ cổ chân."),
        (Shirts, "Áo cầu lông", "Áo thi đấu và tập luyện chất liệu thoáng khí, nhanh khô."),
        (Bottoms, "Quần & váy cầu lông", "Quần short và váy thể thao co giãn, thuận tiện cho các động tác bật nhảy, lunge."),
        (Shuttlecocks, "Quả cầu lông", "Cầu lông lông vũ bán theo ống 12 quả, có nhiều tốc độ phù hợp nhiệt độ và độ cao từng sân."),
        (Bags, "Túi & balo cầu lông", "Túi vợt nhiều ngăn có lớp cách nhiệt bảo vệ vợt, balo đựng giày và phụ kiện."),
        (Strings, "Cước đan vợt", "Cước (dây) đan vợt bán theo sợi 10 m, đủ đan một cây vợt."),
        (Grips, "Quấn cán vợt", "Quấn cán cao su và quấn cán khăn giúp cầm chắc tay, thấm mồ hôi."),
        (Accessories, "Phụ kiện cầu lông", "Tất thể thao, băng chặn mồ hôi và lưới cầu lông.")
    ];

    private static readonly (int Id, string Name, string Description)[] BrandSeeds =
    [
        (Yonex, "Yonex", "Thương hiệu Nhật Bản thành lập năm 1946, nổi tiếng với các dòng vợt Astrox, Nanoflare, Arcsaber, giày Power Cushion và quả cầu Aerosensa."),
        (Victor, "Victor", "Thương hiệu Đài Loan thành lập năm 1968, mạnh về các dòng vợt Thruster, Auraspeed, Brave Sword và quả cầu thi đấu."),
        (LiNing, "Li-Ning", "Thương hiệu Trung Quốc do cựu vận động viên thể dục dụng cụ Lý Ninh sáng lập năm 1990, với các dòng vợt Axforce, Aeronaut, Halbertec."),
        (Apacs, "Apacs", "Thương hiệu Malaysia, nổi tiếng với vợt chịu được mức căng dây cao và các dòng vợt siêu nhẹ, giá hợp lý.")
    ];

    private static VariantSeed[] Single(string code, string label, decimal price, int stock) =>
        [new VariantSeed(code, label, price, stock)];

    private static VariantSeed[] Options(decimal price, params (string Code, string Label, int Stock)[] options) =>
        options.Select(option => new VariantSeed(option.Code, option.Label, price, option.Stock)).ToArray();

    private static VariantSeed[] ShoeSizes(decimal price, params (int Size, int Stock)[] sizes) =>
        sizes.Select(size => new VariantSeed($"{size.Size}", $"Size {size.Size}", price, size.Stock)).ToArray();

    private static VariantSeed[] ClothingSizes(decimal price, params (string Size, int Stock)[] sizes) =>
        sizes.Select(size => new VariantSeed(size.Size, $"Size {size.Size}", price, size.Stock)).ToArray();

    private static readonly ProductSeed[] ProductSeeds =
    [
        // ---- Vợt cầu lông ----
        new(1, "Vợt Yonex Astrox 99 Pro", Rackets, Yonex, "YNX-AX99P", "racket-yonex-astrox-99-pro.jpg",
            "Vợt nặng đầu, thân cứng, thiên về tấn công với những cú đập uy lực. Phù hợp người chơi trình độ khá trở lên, lực cổ tay tốt.",
            Options(4_390_000, ("3UG5", "3U - G5", 4), ("4UG5", "4U - G5", 6)), 10),
        new(2, "Vợt Yonex Astrox 88D Pro", Rackets, Yonex, "YNX-AX88DP", "racket-yonex-astrox-88d-pro.png",
            "Vợt nặng đầu thiết kế cho người đánh cuối sân trong đánh đôi, tối ưu các cú đập và phông cầu sâu. Thân cứng, phù hợp người chơi trình độ khá.",
            Options(4_190_000, ("3UG5", "3U - G5", 3), ("4UG5", "4U - G5", 5)), 40),
        new(3, "Vợt Yonex Nanoflare 800 Pro", Rackets, Yonex, "YNX-NF800P", "racket-yonex-nanoflare-800-pro.png",
            "Vợt nhẹ đầu, khung mỏng giảm lực cản không khí, cho tốc độ vung vợt nhanh. Phù hợp lối đánh phòng thủ phản công và đánh đôi nhanh.",
            Options(4_190_000, ("4UG5", "4U - G5", 5), ("4UG6", "4U - G6", 2)), 15),
        new(4, "Vợt Yonex Arcsaber 11 Pro", Rackets, Yonex, "YNX-ARC11P", "racket-yonex-arcsaber-11-pro.png",
            "Vợt cân bằng, giữ cầu tốt trên mặt lưới giúp kiểm soát điểm rơi chính xác. Phù hợp lối chơi toàn diện, thiên về kỹ thuật.",
            Options(4_290_000, ("3UG5", "3U - G5", 2), ("4UG5", "4U - G5", 4)), 60),
        new(5, "Vợt Yonex Nanoflare 1000Z", Rackets, Yonex, "YNX-NF1000Z", "racket-yonex-nanoflare-1000z.jpg",
            "Vợt nhẹ đầu, thân cứng, tập trung vào tốc độ đánh cầu và các pha đôi công trên lưới. Dành cho người chơi có kỹ thuật tốt.",
            Options(4_590_000, ("4UG5", "4U - G5", 3), ("3UG5", "3U - G5", 0)), 35),
        new(6, "Vợt Yonex Astrox 77 Play", Rackets, Yonex, "YNX-AX77PL", "racket-yonex-astrox-77-play.jpg",
            "Phiên bản phổ thông của dòng Astrox 77, đã đan sẵn cước: hơi nặng đầu, thân trung bình dễ trợ lực. Lựa chọn tốt cho người mới và người chơi phong trào.",
            Single("4UG5", "4U - G5", 1_190_000, 12), 90),
        new(7, "Vợt Yonex Nanoflare 800 Play", Rackets, Yonex, "YNX-NF800PL", "racket-yonex-nanoflare-800-play.jpg",
            "Phiên bản phổ thông của Nanoflare 800, đã đan sẵn cước: nhẹ đầu, vung nhanh. Phù hợp người mới chơi thích lối đánh nhanh, phản tạt.",
            Single("4UG5", "4U - G5", 1_250_000, 10), 85),
        new(8, "Vợt Victor Thruster Ryuga II Pro", Rackets, Victor, "VIC-TKRYU2P", "racket-victor-thruster-ryuga-ii-pro.jpg",
            "Vợt nặng đầu thuộc dòng Thruster thiên công, khung hộp tăng độ ổn định và chịu được mức căng cao, cho cú đập cắm và nặng.",
            Options(3_890_000, ("3UG5", "3U - G5", 3), ("4UG5", "4U - G5", 5)), 20),
        new(9, "Vợt Victor Auraspeed 90K II", Rackets, Victor, "VIC-ARS90K2", "racket-victor-auraspeed-90k-ii.jpg",
            "Dòng vợt tốc độ của Victor, vung nhanh và linh hoạt, phù hợp lối đánh đôi phản tạt, chặn cầu trên lưới.",
            Single("4UG5", "4U - G5", 3_490_000, 4), 70),
        new(10, "Vợt Victor Brave Sword 12", Rackets, Victor, "VIC-BRS12", "racket-victor-brave-sword-12.jpg",
            "Mẫu vợt kinh điển với khung dạng lưỡi kiếm, cân bằng, cho tốc độ và khả năng điều cầu tốt. Phù hợp nhiều lối chơi.",
            Options(2_450_000, ("3UG5", "3U - G5", 5), ("4UG5", "4U - G5", 6)), 120),
        new(11, "Vợt Li-Ning Axforce 80", Rackets, LiNing, "LN-AXF80", "racket-lining-axforce-80.jpg",
            "Vợt nặng đầu thuộc dòng Axforce thiên công, hỗ trợ các cú đập mạnh từ cuối sân.",
            Options(3_790_000, ("3UG5", "3U - G5", 3), ("4UG5", "4U - G5", 4)), 25),
        new(12, "Vợt Li-Ning Aeronaut 9000C", Rackets, LiNing, "LN-AN9000C", "racket-lining-aeronaut-9000c.jpg",
            "Phiên bản thiên công (Combat) của dòng Aeronaut, khung khí động học, cân bằng giữa sức mạnh và tốc độ.",
            Single("4UG5", "4U - G5", 3_690_000, 4), 100),
        new(13, "Vợt Li-Ning Halbertec 8000", Rackets, LiNing, "LN-HB8000", "racket-lining-halbertec-8000.jpg",
            "Vợt cân bằng, toàn diện, kiểm soát cầu tốt ở cả trên lưới và cuối sân. Phù hợp người chơi muốn một cây vợt đa năng.",
            Options(3_890_000, ("3UG5", "3U - G5", 2), ("4UG5", "4U - G5", 3)), 80),
        new(14, "Vợt Apacs Feather Weight 55", Rackets, Apacs, "APC-FW55", "racket-apacs-feather-weight-55.png",
            "Vợt siêu nhẹ (8U), vung rất nhẹ và linh hoạt, giảm tải cho cổ tay. Phù hợp người mới chơi, nữ và người thiên về phản tạt.",
            Single("8UG5", "8U - G5", 1_290_000, 8), 140),

        // ---- Giày cầu lông ----
        new(15, "Giày cầu lông Yonex Power Cushion 65 Z4 - Trắng", Shoes, Yonex, "YNX-SHB65Z4-WH", "shoe-yonex-65z4.jpg",
            "Thế hệ mới của dòng giày bán chạy 65 Z với đệm Power Cushion giảm chấn, đế bám sân tốt, form ôm chân. Phù hợp tập luyện và thi đấu.",
            ShoeSizes(2_790_000, (39, 2), (40, 4), (41, 5), (42, 4), (43, 2)), 12),
        new(16, "Giày cầu lông Yonex Power Cushion Eclipsion Z3 - Trắng", Shoes, Yonex, "YNX-SHBELZ3-WH", "shoe-yonex-eclipsion-z3.png",
            "Giày thiên về độ ổn định, hỗ trợ cổ chân khi di chuyển ngang và đổi hướng đột ngột. Phù hợp người chơi có cường độ cao.",
            ShoeSizes(3_190_000, (40, 2), (41, 3), (42, 3), (43, 1)), 45),
        new(17, "Giày cầu lông Victor A970 Ace - Đen", Shoes, Victor, "VIC-A970ACE-BK", "shoe-victor-a970-ace.jpg",
            "Giày nhẹ, đệm êm, đế cao su chống trơn. Thiết kế hướng tới sự linh hoạt và tốc độ di chuyển.",
            ShoeSizes(2_290_000, (39, 2), (40, 3), (41, 4), (42, 3), (43, 2)), 55),
        new(18, "Giày cầu lông Victor A311 - Đen", Shoes, Victor, "VIC-A311-BK", "shoe-victor-a311.jpg",
            "Giày tầm trung, đế cao su bám sân và êm chân, phù hợp người chơi phong trào tập luyện thường xuyên.",
            ShoeSizes(1_390_000, (39, 3), (40, 4), (41, 5), (42, 4), (43, 0)), 110),
        new(19, "Giày cầu lông Li-Ning Sonic Boom - Trắng", Shoes, LiNing, "LN-SONICB-WH", "shoe-lining-sonic-boom.jpg",
            "Giày unisex nhẹ, đệm êm, đế bám tốt, phối màu trắng dễ kết hợp trang phục.",
            ShoeSizes(2_190_000, (38, 2), (39, 3), (40, 3), (41, 4), (42, 2)), 18),

        // ---- Áo cầu lông ----
        new(20, "Áo cầu lông Yonex 10713EX nam - Xanh navy", Shirts, Yonex, "YNX-10713-NV", "shirt-yonex-10713-navy.jpg",
            "Áo thi đấu cổ tròn, vải polyester thoáng khí, nhanh khô, form regular dễ vận động.",
            ClothingSizes(590_000, ("M", 8), ("L", 10), ("XL", 6)), 30),
        new(21, "Áo cầu lông Yonex YTL12 nữ - Trắng", Shirts, Yonex, "YNX-YTL12-WH", "shirt-yonex-ytl12-white.jpg",
            "Áo nữ form ôm nhẹ, chất liệu mềm mát, thấm hút mồ hôi tốt.",
            ClothingSizes(490_000, ("S", 6), ("M", 8), ("L", 4)), 32),
        new(22, "Áo cầu lông Yonex YTM13 nam - Đỏ", Shirts, Yonex, "YNX-YTM13-RD", "shirt-yonex-ytm13-red.jpg",
            "Áo thi đấu phối màu đỏ nổi bật, vải mè thoáng khí, đường may phẳng hạn chế cọ xát.",
            ClothingSizes(490_000, ("M", 7), ("L", 9), ("XL", 5)), 65),
        new(23, "Áo cầu lông Li-Ning AATN031 nam - Đen", Shirts, LiNing, "LN-AATN031-BK", "shirt-lining-black.jpg",
            "Áo thi đấu họa tiết camo, chất liệu co giãn, nhanh khô.",
            ClothingSizes(450_000, ("M", 5), ("L", 6), ("XL", 3)), 50),
        new(24, "Áo cầu lông Li-Ning AATN013 nam - Xanh dương", Shirts, LiNing, "LN-AATN013-BL", "shirt-lining-blue.jpg",
            "Áo thi đấu phối màu xanh dương và xanh lá, vải mỏng nhẹ, thoáng khí.",
            ClothingSizes(450_000, ("M", 6), ("L", 8), ("XL", 4)), 130),

        // ---- Quần & váy ----
        new(25, "Quần short cầu lông Yonex 15239 nam - Đen", Bottoms, Yonex, "YNX-15239-BK", "shorts-yonex-15239-black.jpg",
            "Quần short vải dệt kim co giãn, lưng thun, nhẹ và thoáng.",
            ClothingSizes(450_000, ("M", 8), ("L", 10), ("XL", 6)), 34),
        new(26, "Váy cầu lông Victor K-56300 nữ - Xanh navy", Bottoms, Victor, "VIC-K56300-NV", "skirt-victor-k56300-navy.webp",
            "Váy xếp ly có quần lót trong, vải co giãn, thoải mái khi di chuyển và bật nhảy.",
            ClothingSizes(490_000, ("S", 5), ("M", 6), ("L", 3)), 75),
        new(27, "Quần short cầu lông Li-Ning AATP043 nam - Đen", Bottoms, LiNing, "LN-AATP043-BK", "shorts-lining-black.jpg",
            "Quần short nhẹ, nhanh khô, phù hợp tập luyện và thi đấu.",
            ClothingSizes(350_000, ("M", 6), ("L", 8), ("XL", 4)), 95),

        // ---- Quả cầu lông ----
        new(28, "Quả cầu lông Yonex Aerosensa 30 (ống 12 quả)", Shuttlecocks, Yonex, "YNX-AS30", "shuttle-yonex-as30.png",
            "Cầu lông vũ, đường bay ổn định, độ bền tốt. Dùng cho tập luyện chất lượng cao và giải phong trào. Chọn tốc độ phù hợp nhiệt độ sân: trời nóng dùng tốc độ thấp hơn.",
            Options(690_000, ("SP2", "Tốc độ 2 (76)", 20), ("SP3", "Tốc độ 3 (77)", 30)), 8),
        new(29, "Quả cầu lông Yonex Aerosensa 50 (ống 12 quả)", Shuttlecocks, Yonex, "YNX-AS50", "shuttle-yonex-as50.jpg",
            "Cầu thi đấu cao cấp, lông vũ chọn lọc, đường bay chuẩn và cảm giác đánh chắc.",
            Options(920_000, ("SP2", "Tốc độ 2 (76)", 10), ("SP3", "Tốc độ 3 (77)", 15)), 85),
        new(30, "Quả cầu lông Victor Champion No.1 (ống 12 quả)", Shuttlecocks, Victor, "VIC-CHNO1", "shuttle-victor-champion-no1.jpg",
            "Cầu lông vũ của Victor dành cho tập luyện và thi đấu phong trào, độ bền khá, giá hợp lý.",
            Single("77", "Tốc độ 77", 450_000, 40), 105),
        new(31, "Quả cầu lông Victor Master Ace (ống 12 quả)", Shuttlecocks, Victor, "VIC-MACE", "shuttle-victor-master-ace.webp",
            "Cầu lông vũ ngỗng cao cấp của Victor, đường bay ổn định, dùng cho tập luyện chuyên sâu và thi đấu.",
            Single("77", "Tốc độ 77", 590_000, 25), 160),

        // ---- Túi & balo ----
        new(32, "Túi vợt Yonex Pro Racquet Bag (6 cây)", Bags, Yonex, "YNX-BA92426", "bag-yonex-pro-6.jpg",
            "Túi vợt 2 ngăn lớn đựng được 6 cây vợt, có lớp cách nhiệt bảo vệ vợt và quai đeo kiểu balo.",
            Options(1_590_000, ("BR", "Màu nâu", 4), ("BK", "Màu đen", 3)), 22),
        new(33, "Balo cầu lông Victor BR5013 - Hồng", Bags, Victor, "VIC-BR5013", "bag-victor-br5013.jpg",
            "Balo có ngăn đựng vợt, ngăn giày riêng thông hơi và nhiều túi phụ kiện, phù hợp đi tập hằng ngày.",
            Single("PK", "Màu hồng", 990_000, 6), 115),
        new(34, "Túi vợt Li-Ning Team 6 cây - Xanh navy", Bags, LiNing, "LN-TEAM6", "bag-lining-team-6.jpg",
            "Túi vợt dáng chữ nhật đựng được 6 cây vợt cùng quần áo, phụ kiện.",
            Single("NV", "Màu xanh navy", 890_000, 5), 125),

        // ---- Cước đan vợt ----
        new(35, "Cước đan vợt Yonex BG65", Strings, Yonex, "YNX-BG65", "string-yonex-bg65.jpg",
            "Sợi cước 0,70 mm nổi tiếng về độ bền, cảm giác đánh chắc. Phù hợp người mới và người chơi đánh nhiều.",
            Options(115_000, ("WH", "Màu trắng", 60), ("YL", "Màu vàng", 40)), 170),
        new(36, "Cước đan vợt Yonex BG66 Ultimax", Strings, Yonex, "YNX-BG66UM", "string-yonex-bg66um.jpg",
            "Sợi cước mảnh 0,65 mm cho độ nảy cao và tiếng đánh vang, được nhiều người chơi trình độ khá lựa chọn.",
            Options(165_000, ("WH", "Màu trắng", 40), ("YL", "Màu vàng", 25)), 145),
        new(37, "Cước đan vợt Yonex BG80", Strings, Yonex, "YNX-BG80", "string-yonex-bg80.jpg",
            "Sợi cước 0,68 mm cứng, cho cú đập mạnh và kiểm soát tốt, phù hợp người chơi thiên công.",
            Options(175_000, ("WH", "Màu trắng", 35), ("YL", "Màu vàng", 20)), 155),
        new(38, "Cước đan vợt Yonex Exbolt 63", Strings, Yonex, "YNX-EXB63", "string-yonex-exbolt63.jpg",
            "Sợi cước mảnh 0,63 mm, cho cảm giác đánh nhanh và chính xác, phù hợp người chơi có kỹ thuật tốt.",
            Options(230_000, ("WH", "Màu trắng", 25), ("YL", "Màu vàng", 15)), 28),
        new(39, "Cước đan vợt Victor VBS-66N", Strings, Victor, "VIC-VBS66N", "string-victor-vbs66n.jpg",
            "Sợi cước 0,66 mm cân bằng giữa độ nảy và độ bền, giá hợp lý.",
            Single("WH", "Màu trắng", 130_000, 30), 135),
        new(40, "Cước đan vợt Li-Ning No.1", Strings, LiNing, "LN-NO1", "string-lining-no1.jpg",
            "Sợi cước mảnh 0,65 mm cho độ nảy tốt và tiếng đánh vang, phù hợp người chơi thiên tốc độ.",
            Single("WH", "Màu trắng", 150_000, 25), 138),

        // ---- Quấn cán ----
        new(41, "Quấn cán Yonex Super Grap AC102EX (vỉ 3 cuộn)", Grips, Yonex, "YNX-AC102EX", "grip-yonex-ac102ex.jpg",
            "Quấn cán cao su mỏng, bám tay và thấm mồ hôi, vỉ 3 cuộn tiết kiệm.",
            Options(120_000, ("WH", "Màu trắng", 30), ("BK", "Màu đen", 25), ("YL", "Màu vàng", 20)), 165),
        new(42, "Quấn cán Victor Fishbone (cuộn lẻ)", Grips, Victor, "VIC-FISHB", "grip-victor-fishbone.jpg",
            "Quấn cán cao su có vân xương cá, cầm chắc tay, chống trượt.",
            Options(45_000, ("RD", "Màu đỏ", 40), ("BK", "Màu đen", 40)), 175),
        new(43, "Quấn cán khăn Yonex AC402EX (cuộn lẻ)", Grips, Yonex, "YNX-AC402EX", "grip-yonex-ac402ex.jpg",
            "Quấn cán vải khăn thấm hút mồ hôi rất tốt, phù hợp người ra nhiều mồ hôi tay.",
            Single("BK", "Màu đen", 60_000, 30), 180),

        // ---- Phụ kiện ----
        new(44, "Tất cầu lông Yonex 19120 (đôi)", Accessories, Yonex, "YNX-19120", "acc-yonex-socks-19120.jpg",
            "Tất thể thao cổ trung thiết kế 3D ôm chân, đệm dày ở gót và mũi chân, giảm ma sát khi di chuyển.",
            Options(150_000, ("M", "Cỡ 39-42", 30), ("L", "Cỡ 42-45", 25)), 168),
        new(45, "Băng chặn mồ hôi cổ tay Yonex AC493", Accessories, Yonex, "YNX-AC493", "acc-yonex-wristband-ac493.jpg",
            "Băng cổ tay vải cotton co giãn, thấm mồ hôi, giữ tay khô khi thi đấu.",
            Options(120_000, ("WH", "Màu trắng", 20), ("BK", "Màu đen", 20)), 172),
        new(46, "Lưới cầu lông Victor C-7004", Accessories, Victor, "VIC-C7004", "acc-victor-net-c7004.webp",
            "Lưới cầu lông thi đấu kích thước tiêu chuẩn dài 6,1 m, viền trên bằng vải bền, dùng cho sân thi đấu và sân tập.",
            Single("STD", "Tiêu chuẩn 6,1 m", 650_000, 5), 185)
    ];

    /// <summary>Seeded variants with the ids <see cref="Apply"/> assigns (used to seed variant specifications).</summary>
    internal static IReadOnlyList<(int VariantId, int ProductId, int CategoryId, string Label)> SeedVariants()
    {
        var result = new List<(int, int, int, string)>();
        var variantId = 1;
        foreach (var seed in ProductSeeds)
        {
            foreach (var variant in seed.Variants)
            {
                result.Add((variantId++, seed.Id, seed.CategoryId, variant.Label));
            }
        }

        return result;
    }

    public static void Apply(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>().HasData(CategorySeeds.Select(seed => new Category
        {
            Id = seed.Id,
            Name = seed.Name,
            Description = seed.Description,
            IsActive = true,
            CreatedAt = SeedDate,
            UpdatedAt = SeedDate
        }));

        modelBuilder.Entity<Brand>().HasData(BrandSeeds.Select(seed => new Brand
        {
            Id = seed.Id,
            Name = seed.Name,
            Description = seed.Description,
            IsActive = true,
            // Window for reporting defects found on receipt. No brand publishes this officially;
            // defaults follow Vietnamese shop practice (Victor 7 days, others 3) and are editable by admins.
            ReceiptDefectDays = seed.Id == Victor ? 7 : 3,
            CreatedAt = SeedDate,
            UpdatedAt = SeedDate
        }));

        var products = new List<Product>();
        var variants = new List<ProductVariant>();
        var images = new List<ProductImage>();
        var inventory = new List<InventoryTransaction>();
        var variantId = 1;

        foreach (var seed in ProductSeeds)
        {
            // Newer products have a smaller DaysSinceLaunch, so "latest products" on the home page
            // shows flagship rackets, shoes and shuttlecocks first.
            var createdAt = SeedDate.AddDays(200 - seed.DaysSinceLaunch);
            products.Add(new Product
            {
                Id = seed.Id,
                Name = seed.Name,
                Description = seed.Description,
                CategoryId = seed.CategoryId,
                BrandId = seed.BrandId,
                IsActive = true,
                CreatedAt = createdAt,
                UpdatedAt = createdAt
            });

            images.Add(new ProductImage
            {
                Id = seed.Id,
                ProductId = seed.Id,
                ImageUrl = $"/img/product/catalog/{seed.Image}",
                IsPrimary = true,
                SortOrder = 1
            });

            foreach (var variant in seed.Variants)
            {
                var id = variantId++;
                variants.Add(new ProductVariant
                {
                    Id = id,
                    ProductId = seed.Id,
                    SKU = $"{seed.SkuPrefix}-{variant.Code}",
                    VariantLabel = variant.Label,
                    Price = variant.Price,
                    StockQuantity = variant.Stock,
                    IsActive = true
                });

                if (variant.Stock > 0)
                {
                    inventory.Add(new InventoryTransaction
                    {
                        Id = id,
                        ProductVariantId = id,
                        Type = InventoryTransactionType.Import,
                        Quantity = variant.Stock,
                        Note = "Tồn kho đầu kỳ",
                        CreatedAt = createdAt
                    });
                }
            }
        }

        modelBuilder.Entity<Product>().HasData(products);
        modelBuilder.Entity<ProductVariant>().HasData(variants);
        modelBuilder.Entity<ProductImage>().HasData(images);
        modelBuilder.Entity<InventoryTransaction>().HasData(inventory);
    }
}
