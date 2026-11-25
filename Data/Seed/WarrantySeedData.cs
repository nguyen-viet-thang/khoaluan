using Microsoft.EntityFrameworkCore;
using PopPop.Data.Entities;
using PopPop.Data.Entities.Enums;

namespace PopPop.Data.Seed;

/// <summary>
/// Default warranty policies and defect catalogue, compiled from the brands' published policies
/// (Yonex US warranty page, Li-Ning LN Protect) and common practice of Vietnamese badminton shops.
/// Durations marked "cần xác nhận" must be confirmed with the Vietnamese distributor; admins can edit them.
/// </summary>
internal static class WarrantySeedData
{
    private static readonly DateTime SeedDate = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private const int Rackets = 1, Shoes = 2, Shirts = 3, Bottoms = 4, Shuttlecocks = 5, Bags = 6, Strings = 7, Grips = 8, Accessories = 9;
    private const int Yonex = 1, Victor = 2, LiNing = 3, Apacs = 4;

    private const string YonexUrl = "https://us.yonex.com/pages/warranty";
    private const string LiNingUrl = "https://in.lining.studio/pages/ln-protect-policy";
    private const string DealerUrl = "https://www.badmintonwarehouse.com/blogs/news/yonex-badminton-racket-warranty-a-comprehensive-guide";
    private const string ShopShoeUrl = "https://loansport.vn/ho-tro/chinh-sach-bao-hanh-giay-cau-long/56";
    private const string VnShopUrl = "https://xbsports.vn/blogs/news/quy-dinh-bao-hanh-vot-cau-long-chinh-sach-tu-yonex-li-ning-va-victor";

    private const DefectCoverage W = DefectCoverage.Warranty, R = DefectCoverage.ReceiptDefect, N = DefectCoverage.NotCovered;

    private const string RacketEvidence = "Quay toàn bộ vợt, cận cảnh chỗ lỗi, xoay quanh khung để thấy không có vết va đập; quay tem/số serial. KHÔNG cắt dây.";
    private const string UnboxingEvidence = "Video mở hộp quay liền mạch, không cắt ghép, bắt đầu từ lúc gói hàng còn nguyên niêm phong; cận cảnh lỗi hoặc thông tin sai lệch.";

    private static readonly (string Code, int CategoryId, string Name, string? Recognition, string? Evidence, DefectCoverage Coverage, string Source, string? Url)[] Defects =
    [
        // ---- Vợt ----
        ("V01", Rackets, "Khung vợt nứt/gãy không do va chạm", "Vết nứt mảnh; quanh điểm nứt không có vết trầy, móp hay bong sơn.", RacketEvidence, W, "Yonex, Li-Ning: lỗi vật liệu/sản xuất ở khung", YonexUrl),
        ("V02", Rackets, "Thân vợt hoặc cổ vợt nứt/gãy không do va chạm", "Như V01, vị trí ở thân (shaft) hoặc chỗ nối thân với khung.", RacketEvidence, W, "Li-Ning: bảo hành thân vợt", LiNingUrl),
        ("V03", Rackets, "Cán vợt lỏng, xoay, tách khỏi thân hoặc nứt cán", "Cán xoay hoặc kêu lạo xạo khi cầm, không do tháo/sửa.", "Quay cảnh vặn nhẹ cán để thấy bị lỏng; quay tem/số serial.", W, "Li-Ning: bảo hành cán (grip)", LiNingUrl),
        ("V04", Rackets, "Nứt tại lỗ xỏ dây khi đan đúng mức căng khuyến nghị", "Vết nứt xuất phát từ lỗ xỏ dây, đan không vượt mức căng in trên vợt.", "Cận cảnh lỗ bị nứt; ảnh tem mức căng tối đa trên vợt; ghi mức căng đã đan vào mô tả.", W, "Đại lý (cần nhà phân phối xác nhận)", DealerUrl),
        ("V05", Rackets, "Giao sai mẫu, trọng lượng (3U/4U) hoặc cỡ cán so với đơn", "Thông số in trên vợt khác với đơn hàng.", UnboxingEvidence, R, "Trách nhiệm của cửa hàng", null),
        ("V06", Rackets, "Vợt trầy, nứt, móp ngay khi mở hộp", "Phát hiện ngay khi mở gói hàng.", UnboxingEvidence, R, "Trách nhiệm của cửa hàng/vận chuyển", null),
        ("V91", Rackets, "Gãy/nứt do va chạm (đập vợt, va vào sàn/tường)", "Có vết trầy, móp, bong sơn tại điểm gãy.", null, N, "Yonex, Li-Ning: không bảo hành va chạm", YonexUrl),
        ("V92", Rackets, "Gãy do căng dây quá mức hoặc đan sai sơ đồ của hãng", null, null, N, "Yonex, Li-Ning", YonexUrl),
        ("V93", Rackets, "Đứt dây; mòn hoặc rách quấn cán", null, null, N, "Yonex, Li-Ning: hàng tiêu hao", LiNingUrl),
        ("V94", Rackets, "Đã cắt dây trước khi gửi, tự sửa hoặc dán lại", null, null, N, "Yonex, Victor", YonexUrl),
        ("V95", Rackets, "Trầy xước, bạc màu do sử dụng; hư do để nơi quá nóng", null, null, N, "Yonex", YonexUrl),

        // ---- Giày ----
        ("G01", Shoes, "Bong keo, đế tách khỏi thân giày bất thường", "Khe hở giữa đế và thân, keo không bám, đế chưa mòn.", "Cận cảnh chỗ bong; quay toàn bộ đế (chứng minh chưa mòn); tem size trong lưỡi gà.", W, "Thông lệ cửa hàng (cần nhà phân phối xác nhận)", ShopShoeUrl),
        ("G02", Shoes, "Bung chỉ đường may thân giày", "Chỉ tuột ở đường may, vải xung quanh không rách.", "Cận cảnh đường may bị bung; tem size.", W, "Thông lệ cửa hàng (cần nhà phân phối xác nhận)", ShopShoeUrl),
        ("G03", Shoes, "Giao sai size/mẫu; hai chiếc khác size hoặc khác mẫu", "Tem size trong lưỡi gà khác với đơn hàng.", UnboxingEvidence + " Quay tem size của cả hai chiếc.", R, "Trách nhiệm của cửa hàng", null),
        ("G04", Shoes, "Lỗi ngoại quan khi mở hộp: rách, bẩn, lem keo, bong sẵn", null, UnboxingEvidence, R, "Trách nhiệm của cửa hàng", null),
        ("G91", Shoes, "Mòn đế, trầy, rách do va chạm; hư do ngâm nước, phơi nắng; đã tự dán keo nơi khác", null, null, N, "Thông lệ cửa hàng", ShopShoeUrl),
        ("G92", Shoes, "Không vừa chân, không thích mẫu (khách tự chọn size)", null, null, N, "Chính sách PôpPôp: không đổi vì không vừa ý", null),

        // ---- Áo ----
        ("A01", Shirts, "Lỗi may, thủng, bẩn khi nhận hàng", null, UnboxingEvidence, R, "Trách nhiệm của cửa hàng", null),
        ("A02", Shirts, "Giao sai size/màu so với đơn", "Tem size/màu khác với đơn hàng.", UnboxingEvidence, R, "Trách nhiệm của cửa hàng", null),
        ("A91", Shirts, "Không vừa, không thích", null, null, N, "Chính sách PôpPôp: không đổi vì không vừa ý", null),
        ("A92", Shirts, "Co rút, phai màu do giặt sai cách", null, null, N, "Hao mòn do sử dụng", null),

        // ---- Quần & váy ----
        ("Q01", Bottoms, "Lỗi may, thủng, bẩn khi nhận hàng", null, UnboxingEvidence, R, "Trách nhiệm của cửa hàng", null),
        ("Q02", Bottoms, "Giao sai size/màu so với đơn", "Tem size/màu khác với đơn hàng.", UnboxingEvidence, R, "Trách nhiệm của cửa hàng", null),
        ("Q91", Bottoms, "Không vừa, không thích", null, null, N, "Chính sách PôpPôp: không đổi vì không vừa ý", null),
        ("Q92", Bottoms, "Co rút, phai màu do giặt sai cách", null, null, N, "Hao mòn do sử dụng", null),

        // ---- Quả cầu ----
        ("C01", Shuttlecocks, "Ống cầu thiếu quả (ít hơn 12 quả)", null, UnboxingEvidence + " Đếm từng quả trước camera.", R, "Trách nhiệm của cửa hàng", null),
        ("C02", Shuttlecocks, "Quả cầu gãy lông, bung đầu ngay khi mở ống", null, UnboxingEvidence, R, "Trách nhiệm của cửa hàng", null),
        ("C03", Shuttlecocks, "Giao sai loại hoặc tốc độ cầu", "Tốc độ in trên ống khác với đơn hàng.", UnboxingEvidence, R, "Trách nhiệm của cửa hàng", null),
        ("C91", Shuttlecocks, "Cầu hỏng sau khi đã đánh", null, null, N, "Li-Ning: quả cầu không bảo hành", LiNingUrl),

        // ---- Túi & balo ----
        ("T01", Bags, "Bung hoặc đứt đường may do lỗi may", "Chỉ tuột, vải không rách.", "Cận cảnh đường may; quay toàn bộ túi.", W, "Li-Ning: túi được bảo hành lỗi sản xuất", LiNingUrl),
        ("T02", Bags, "Khóa kéo hỏng do lỗi sản xuất (răng khóa tách, đầu khóa gãy khi dùng bình thường)", null, "Quay cảnh kéo khóa bị lỗi.", W, "Li-Ning: túi được bảo hành lỗi sản xuất", LiNingUrl),
        ("T03", Bags, "Giao sai mẫu/màu hoặc lỗi khi mở hộp", null, UnboxingEvidence, R, "Trách nhiệm của cửa hàng", null),
        ("T91", Bags, "Rách do vật sắc, quá tải, dùng sai mục đích; mài mòn", null, null, N, "Li-Ning", LiNingUrl),

        // ---- Cước ----
        ("D01", Strings, "Giao sai loại hoặc màu cước", null, UnboxingEvidence, R, "Trách nhiệm của cửa hàng", null),
        ("D02", Strings, "Sợi cước đứt sẵn hoặc bao bì rách khi mở gói", null, UnboxingEvidence, R, "Trách nhiệm của cửa hàng", null),
        ("D91", Strings, "Đứt dây khi sử dụng", null, null, N, "Li-Ning, Yonex: hàng tiêu hao", LiNingUrl),

        // ---- Quấn cán ----
        ("K01", Grips, "Giao sai loại hoặc màu quấn cán", null, UnboxingEvidence, R, "Trách nhiệm của cửa hàng", null),
        ("K02", Grips, "Bao bì rách hoặc quấn cán lỗi khi mở gói", null, UnboxingEvidence, R, "Trách nhiệm của cửa hàng", null),
        ("K91", Grips, "Mòn, rách khi sử dụng", null, null, N, "Li-Ning: hàng tiêu hao", LiNingUrl),

        // ---- Phụ kiện ----
        ("P01", Accessories, "Lỗi hoặc giao sai khi nhận hàng", null, UnboxingEvidence, R, "Trách nhiệm của cửa hàng", null),
        ("P91", Accessories, "Hao mòn khi sử dụng", null, null, N, "Hao mòn do sử dụng", null)
    ];

    private static readonly (int BrandId, int CategoryId, int Days, string Covered, string Notes, string? Url)[] Policies =
    [
        (Yonex, Rackets, 90, "Khung, thân, cán vợt", "Cửa hàng tại VN (nhà phân phối Sunrise) áp dụng 3 tháng; chính sách Yonex Mỹ là 1 năm (chỉ tại Mỹ). Cần xác nhận với nhà phân phối.", VnShopUrl),
        (Victor, Rackets, 60, "Khung, thân, cán vợt", "Cửa hàng tại VN ghi 60 ngày; Victor Mỹ 1 năm (qua đại lý). Cần xác nhận với nhà phân phối.", VnShopUrl),
        (LiNing, Rackets, 90, "Khung, thân, cán vợt", "Chính sách chính thức LN Protect: 90 ngày kể từ ngày giao.", LiNingUrl),
        (Apacs, Rackets, 0, "—", "Chưa tìm thấy chính sách chính thức của Apacs; tạm chỉ nhận lỗi khi nhận hàng.", null),
        (Yonex, Shoes, 30, "Keo, đế, đường may", "Hãng không công bố bảo hành giày; theo thông lệ cửa hàng tại VN. Cần xác nhận với nhà phân phối.", ShopShoeUrl),
        (Victor, Shoes, 30, "Keo, đế, đường may", "Hãng không công bố bảo hành giày; theo thông lệ cửa hàng tại VN. Cần xác nhận với nhà phân phối.", ShopShoeUrl),
        (LiNing, Shoes, 30, "Keo, đế, đường may", "Li-Ning đang tạm dừng bảo hành giày (LN Protect); theo thông lệ cửa hàng tại VN. Cần xác nhận.", ShopShoeUrl),
        (LiNing, Bags, 90, "Đường may, khóa kéo (lỗi sản xuất)", "Chính sách chính thức LN Protect: túi 90 ngày kể từ ngày giao.", LiNingUrl)
    ];

    public static void Apply(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DefectType>().HasData(Defects.Select((defect, index) => new DefectType
        {
            Id = index + 1,
            Code = defect.Code,
            CategoryId = defect.CategoryId,
            Name = defect.Name,
            Recognition = defect.Recognition,
            EvidenceGuide = defect.Evidence,
            Coverage = defect.Coverage,
            Source = defect.Source,
            SourceUrl = defect.Url,
            SortOrder = index + 1,
            IsActive = true
        }));

        modelBuilder.Entity<WarrantyPolicy>().HasData(Policies.Select((policy, index) => new WarrantyPolicy
        {
            Id = index + 1,
            BrandId = policy.BrandId,
            CategoryId = policy.CategoryId,
            WarrantyDays = policy.Days,
            CoveredParts = policy.Covered,
            Notes = policy.Notes,
            SourceUrl = policy.Url,
            UpdatedAt = SeedDate
        }));
    }
}
