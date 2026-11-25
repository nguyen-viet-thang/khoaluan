using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PopPop.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWarrantyClaims : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReceiptDefectDays",
                table: "Brands",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "DefectTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Recognition = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EvidenceGuide = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Coverage = table.Column<int>(type: "int", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    SourceUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DefectTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DefectTypes_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WarrantyPolicies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BrandId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    WarrantyDays = table.Column<int>(type: "int", nullable: false),
                    CoveredParts = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SourceUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarrantyPolicies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WarrantyPolicies_Brands_BrandId",
                        column: x => x.BrandId,
                        principalTable: "Brands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WarrantyPolicies_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WarrantyClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    OrderItemId = table.Column<int>(type: "int", nullable: false),
                    DefectTypeId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeadlineAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Resolution = table.Column<int>(type: "int", nullable: true),
                    RefundAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    StaffNote = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarrantyClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WarrantyClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WarrantyClaims_DefectTypes_DefectTypeId",
                        column: x => x.DefectTypeId,
                        principalTable: "DefectTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WarrantyClaims_OrderItems_OrderItemId",
                        column: x => x.OrderItemId,
                        principalTable: "OrderItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WarrantyClaims_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WarrantyClaimHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClaimId = table.Column<int>(type: "int", nullable: false),
                    FromStatus = table.Column<int>(type: "int", nullable: true),
                    ToStatus = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ActorUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActorName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ByStaff = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarrantyClaimHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WarrantyClaimHistory_WarrantyClaims_ClaimId",
                        column: x => x.ClaimId,
                        principalTable: "WarrantyClaims",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WarrantyClaimMedia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClaimId = table.Column<int>(type: "int", nullable: false),
                    Kind = table.Column<int>(type: "int", nullable: false),
                    StoredFileName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    UploadedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UploadedByStaff = table.Column<bool>(type: "bit", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarrantyClaimMedia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WarrantyClaimMedia_WarrantyClaims_ClaimId",
                        column: x => x.ClaimId,
                        principalTable: "WarrantyClaims",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: 1,
                column: "ReceiptDefectDays",
                value: 3);

            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: 2,
                column: "ReceiptDefectDays",
                value: 7);

            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: 3,
                column: "ReceiptDefectDays",
                value: 3);

            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: 4,
                column: "ReceiptDefectDays",
                value: 3);

            migrationBuilder.InsertData(
                table: "DefectTypes",
                columns: new[] { "Id", "CategoryId", "Code", "Coverage", "EvidenceGuide", "IsActive", "Name", "Recognition", "SortOrder", "Source", "SourceUrl" },
                values: new object[,]
                {
                    { 1, 1, "V01", 1, "Quay toàn bộ vợt, cận cảnh chỗ lỗi, xoay quanh khung để thấy không có vết va đập; quay tem/số serial. KHÔNG cắt dây.", true, "Khung vợt nứt/gãy không do va chạm", "Vết nứt mảnh; quanh điểm nứt không có vết trầy, móp hay bong sơn.", 1, "Yonex, Li-Ning: lỗi vật liệu/sản xuất ở khung", "https://us.yonex.com/pages/warranty" },
                    { 2, 1, "V02", 1, "Quay toàn bộ vợt, cận cảnh chỗ lỗi, xoay quanh khung để thấy không có vết va đập; quay tem/số serial. KHÔNG cắt dây.", true, "Thân vợt hoặc cổ vợt nứt/gãy không do va chạm", "Như V01, vị trí ở thân (shaft) hoặc chỗ nối thân với khung.", 2, "Li-Ning: bảo hành thân vợt", "https://in.lining.studio/pages/ln-protect-policy" },
                    { 3, 1, "V03", 1, "Quay cảnh vặn nhẹ cán để thấy bị lỏng; quay tem/số serial.", true, "Cán vợt lỏng, xoay, tách khỏi thân hoặc nứt cán", "Cán xoay hoặc kêu lạo xạo khi cầm, không do tháo/sửa.", 3, "Li-Ning: bảo hành cán (grip)", "https://in.lining.studio/pages/ln-protect-policy" },
                    { 4, 1, "V04", 1, "Cận cảnh lỗ bị nứt; ảnh tem mức căng tối đa trên vợt; ghi mức căng đã đan vào mô tả.", true, "Nứt tại lỗ xỏ dây khi đan đúng mức căng khuyến nghị", "Vết nứt xuất phát từ lỗ xỏ dây, đan không vượt mức căng in trên vợt.", 4, "Đại lý (cần nhà phân phối xác nhận)", "https://www.badmintonwarehouse.com/blogs/news/yonex-badminton-racket-warranty-a-comprehensive-guide" },
                    { 5, 1, "V05", 2, "Video mở hộp quay liền mạch, không cắt ghép, bắt đầu từ lúc gói hàng còn nguyên niêm phong; cận cảnh lỗi hoặc thông tin sai lệch.", true, "Giao sai mẫu, trọng lượng (3U/4U) hoặc cỡ cán so với đơn", "Thông số in trên vợt khác với đơn hàng.", 5, "Trách nhiệm của cửa hàng", null },
                    { 6, 1, "V06", 2, "Video mở hộp quay liền mạch, không cắt ghép, bắt đầu từ lúc gói hàng còn nguyên niêm phong; cận cảnh lỗi hoặc thông tin sai lệch.", true, "Vợt trầy, nứt, móp ngay khi mở hộp", "Phát hiện ngay khi mở gói hàng.", 6, "Trách nhiệm của cửa hàng/vận chuyển", null },
                    { 7, 1, "V91", 3, null, true, "Gãy/nứt do va chạm (đập vợt, va vào sàn/tường)", "Có vết trầy, móp, bong sơn tại điểm gãy.", 7, "Yonex, Li-Ning: không bảo hành va chạm", "https://us.yonex.com/pages/warranty" },
                    { 8, 1, "V92", 3, null, true, "Gãy do căng dây quá mức hoặc đan sai sơ đồ của hãng", null, 8, "Yonex, Li-Ning", "https://us.yonex.com/pages/warranty" },
                    { 9, 1, "V93", 3, null, true, "Đứt dây; mòn hoặc rách quấn cán", null, 9, "Yonex, Li-Ning: hàng tiêu hao", "https://in.lining.studio/pages/ln-protect-policy" },
                    { 10, 1, "V94", 3, null, true, "Đã cắt dây trước khi gửi, tự sửa hoặc dán lại", null, 10, "Yonex, Victor", "https://us.yonex.com/pages/warranty" },
                    { 11, 1, "V95", 3, null, true, "Trầy xước, bạc màu do sử dụng; hư do để nơi quá nóng", null, 11, "Yonex", "https://us.yonex.com/pages/warranty" },
                    { 12, 2, "G01", 1, "Cận cảnh chỗ bong; quay toàn bộ đế (chứng minh chưa mòn); tem size trong lưỡi gà.", true, "Bong keo, đế tách khỏi thân giày bất thường", "Khe hở giữa đế và thân, keo không bám, đế chưa mòn.", 12, "Thông lệ cửa hàng (cần nhà phân phối xác nhận)", "https://loansport.vn/ho-tro/chinh-sach-bao-hanh-giay-cau-long/56" },
                    { 13, 2, "G02", 1, "Cận cảnh đường may bị bung; tem size.", true, "Bung chỉ đường may thân giày", "Chỉ tuột ở đường may, vải xung quanh không rách.", 13, "Thông lệ cửa hàng (cần nhà phân phối xác nhận)", "https://loansport.vn/ho-tro/chinh-sach-bao-hanh-giay-cau-long/56" },
                    { 14, 2, "G03", 2, "Video mở hộp quay liền mạch, không cắt ghép, bắt đầu từ lúc gói hàng còn nguyên niêm phong; cận cảnh lỗi hoặc thông tin sai lệch. Quay tem size của cả hai chiếc.", true, "Giao sai size/mẫu; hai chiếc khác size hoặc khác mẫu", "Tem size trong lưỡi gà khác với đơn hàng.", 14, "Trách nhiệm của cửa hàng", null },
                    { 15, 2, "G04", 2, "Video mở hộp quay liền mạch, không cắt ghép, bắt đầu từ lúc gói hàng còn nguyên niêm phong; cận cảnh lỗi hoặc thông tin sai lệch.", true, "Lỗi ngoại quan khi mở hộp: rách, bẩn, lem keo, bong sẵn", null, 15, "Trách nhiệm của cửa hàng", null },
                    { 16, 2, "G91", 3, null, true, "Mòn đế, trầy, rách do va chạm; hư do ngâm nước, phơi nắng; đã tự dán keo nơi khác", null, 16, "Thông lệ cửa hàng", "https://loansport.vn/ho-tro/chinh-sach-bao-hanh-giay-cau-long/56" },
                    { 17, 2, "G92", 3, null, true, "Không vừa chân, không thích mẫu (khách tự chọn size)", null, 17, "Chính sách PôpPôp: không đổi vì không vừa ý", null },
                    { 18, 3, "A01", 2, "Video mở hộp quay liền mạch, không cắt ghép, bắt đầu từ lúc gói hàng còn nguyên niêm phong; cận cảnh lỗi hoặc thông tin sai lệch.", true, "Lỗi may, thủng, bẩn khi nhận hàng", null, 18, "Trách nhiệm của cửa hàng", null },
                    { 19, 3, "A02", 2, "Video mở hộp quay liền mạch, không cắt ghép, bắt đầu từ lúc gói hàng còn nguyên niêm phong; cận cảnh lỗi hoặc thông tin sai lệch.", true, "Giao sai size/màu so với đơn", "Tem size/màu khác với đơn hàng.", 19, "Trách nhiệm của cửa hàng", null },
                    { 20, 3, "A91", 3, null, true, "Không vừa, không thích", null, 20, "Chính sách PôpPôp: không đổi vì không vừa ý", null },
                    { 21, 3, "A92", 3, null, true, "Co rút, phai màu do giặt sai cách", null, 21, "Hao mòn do sử dụng", null },
                    { 22, 4, "Q01", 2, "Video mở hộp quay liền mạch, không cắt ghép, bắt đầu từ lúc gói hàng còn nguyên niêm phong; cận cảnh lỗi hoặc thông tin sai lệch.", true, "Lỗi may, thủng, bẩn khi nhận hàng", null, 22, "Trách nhiệm của cửa hàng", null },
                    { 23, 4, "Q02", 2, "Video mở hộp quay liền mạch, không cắt ghép, bắt đầu từ lúc gói hàng còn nguyên niêm phong; cận cảnh lỗi hoặc thông tin sai lệch.", true, "Giao sai size/màu so với đơn", "Tem size/màu khác với đơn hàng.", 23, "Trách nhiệm của cửa hàng", null },
                    { 24, 4, "Q91", 3, null, true, "Không vừa, không thích", null, 24, "Chính sách PôpPôp: không đổi vì không vừa ý", null },
                    { 25, 4, "Q92", 3, null, true, "Co rút, phai màu do giặt sai cách", null, 25, "Hao mòn do sử dụng", null },
                    { 26, 5, "C01", 2, "Video mở hộp quay liền mạch, không cắt ghép, bắt đầu từ lúc gói hàng còn nguyên niêm phong; cận cảnh lỗi hoặc thông tin sai lệch. Đếm từng quả trước camera.", true, "Ống cầu thiếu quả (ít hơn 12 quả)", null, 26, "Trách nhiệm của cửa hàng", null },
                    { 27, 5, "C02", 2, "Video mở hộp quay liền mạch, không cắt ghép, bắt đầu từ lúc gói hàng còn nguyên niêm phong; cận cảnh lỗi hoặc thông tin sai lệch.", true, "Quả cầu gãy lông, bung đầu ngay khi mở ống", null, 27, "Trách nhiệm của cửa hàng", null },
                    { 28, 5, "C03", 2, "Video mở hộp quay liền mạch, không cắt ghép, bắt đầu từ lúc gói hàng còn nguyên niêm phong; cận cảnh lỗi hoặc thông tin sai lệch.", true, "Giao sai loại hoặc tốc độ cầu", "Tốc độ in trên ống khác với đơn hàng.", 28, "Trách nhiệm của cửa hàng", null },
                    { 29, 5, "C91", 3, null, true, "Cầu hỏng sau khi đã đánh", null, 29, "Li-Ning: quả cầu không bảo hành", "https://in.lining.studio/pages/ln-protect-policy" },
                    { 30, 6, "T01", 1, "Cận cảnh đường may; quay toàn bộ túi.", true, "Bung hoặc đứt đường may do lỗi may", "Chỉ tuột, vải không rách.", 30, "Li-Ning: túi được bảo hành lỗi sản xuất", "https://in.lining.studio/pages/ln-protect-policy" },
                    { 31, 6, "T02", 1, "Quay cảnh kéo khóa bị lỗi.", true, "Khóa kéo hỏng do lỗi sản xuất (răng khóa tách, đầu khóa gãy khi dùng bình thường)", null, 31, "Li-Ning: túi được bảo hành lỗi sản xuất", "https://in.lining.studio/pages/ln-protect-policy" },
                    { 32, 6, "T03", 2, "Video mở hộp quay liền mạch, không cắt ghép, bắt đầu từ lúc gói hàng còn nguyên niêm phong; cận cảnh lỗi hoặc thông tin sai lệch.", true, "Giao sai mẫu/màu hoặc lỗi khi mở hộp", null, 32, "Trách nhiệm của cửa hàng", null },
                    { 33, 6, "T91", 3, null, true, "Rách do vật sắc, quá tải, dùng sai mục đích; mài mòn", null, 33, "Li-Ning", "https://in.lining.studio/pages/ln-protect-policy" },
                    { 34, 7, "D01", 2, "Video mở hộp quay liền mạch, không cắt ghép, bắt đầu từ lúc gói hàng còn nguyên niêm phong; cận cảnh lỗi hoặc thông tin sai lệch.", true, "Giao sai loại hoặc màu cước", null, 34, "Trách nhiệm của cửa hàng", null },
                    { 35, 7, "D02", 2, "Video mở hộp quay liền mạch, không cắt ghép, bắt đầu từ lúc gói hàng còn nguyên niêm phong; cận cảnh lỗi hoặc thông tin sai lệch.", true, "Sợi cước đứt sẵn hoặc bao bì rách khi mở gói", null, 35, "Trách nhiệm của cửa hàng", null },
                    { 36, 7, "D91", 3, null, true, "Đứt dây khi sử dụng", null, 36, "Li-Ning, Yonex: hàng tiêu hao", "https://in.lining.studio/pages/ln-protect-policy" },
                    { 37, 8, "K01", 2, "Video mở hộp quay liền mạch, không cắt ghép, bắt đầu từ lúc gói hàng còn nguyên niêm phong; cận cảnh lỗi hoặc thông tin sai lệch.", true, "Giao sai loại hoặc màu quấn cán", null, 37, "Trách nhiệm của cửa hàng", null },
                    { 38, 8, "K02", 2, "Video mở hộp quay liền mạch, không cắt ghép, bắt đầu từ lúc gói hàng còn nguyên niêm phong; cận cảnh lỗi hoặc thông tin sai lệch.", true, "Bao bì rách hoặc quấn cán lỗi khi mở gói", null, 38, "Trách nhiệm của cửa hàng", null },
                    { 39, 8, "K91", 3, null, true, "Mòn, rách khi sử dụng", null, 39, "Li-Ning: hàng tiêu hao", "https://in.lining.studio/pages/ln-protect-policy" },
                    { 40, 9, "P01", 2, "Video mở hộp quay liền mạch, không cắt ghép, bắt đầu từ lúc gói hàng còn nguyên niêm phong; cận cảnh lỗi hoặc thông tin sai lệch.", true, "Lỗi hoặc giao sai khi nhận hàng", null, 40, "Trách nhiệm của cửa hàng", null },
                    { 41, 9, "P91", 3, null, true, "Hao mòn khi sử dụng", null, 41, "Hao mòn do sử dụng", null }
                });

            migrationBuilder.InsertData(
                table: "WarrantyPolicies",
                columns: new[] { "Id", "BrandId", "CategoryId", "CoveredParts", "Notes", "SourceUrl", "UpdatedAt", "WarrantyDays" },
                values: new object[,]
                {
                    { 1, 1, 1, "Khung, thân, cán vợt", "Cửa hàng tại VN (nhà phân phối Sunrise) áp dụng 3 tháng; chính sách Yonex Mỹ là 1 năm (chỉ tại Mỹ). Cần xác nhận với nhà phân phối.", "https://xbsports.vn/blogs/news/quy-dinh-bao-hanh-vot-cau-long-chinh-sach-tu-yonex-li-ning-va-victor", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 90 },
                    { 2, 2, 1, "Khung, thân, cán vợt", "Cửa hàng tại VN ghi 60 ngày; Victor Mỹ 1 năm (qua đại lý). Cần xác nhận với nhà phân phối.", "https://xbsports.vn/blogs/news/quy-dinh-bao-hanh-vot-cau-long-chinh-sach-tu-yonex-li-ning-va-victor", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 60 },
                    { 3, 3, 1, "Khung, thân, cán vợt", "Chính sách chính thức LN Protect: 90 ngày kể từ ngày giao.", "https://in.lining.studio/pages/ln-protect-policy", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 90 },
                    { 4, 4, 1, "—", "Chưa tìm thấy chính sách chính thức của Apacs; tạm chỉ nhận lỗi khi nhận hàng.", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 5, 1, 2, "Keo, đế, đường may", "Hãng không công bố bảo hành giày; theo thông lệ cửa hàng tại VN. Cần xác nhận với nhà phân phối.", "https://loansport.vn/ho-tro/chinh-sach-bao-hanh-giay-cau-long/56", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 30 },
                    { 6, 2, 2, "Keo, đế, đường may", "Hãng không công bố bảo hành giày; theo thông lệ cửa hàng tại VN. Cần xác nhận với nhà phân phối.", "https://loansport.vn/ho-tro/chinh-sach-bao-hanh-giay-cau-long/56", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 30 },
                    { 7, 3, 2, "Keo, đế, đường may", "Li-Ning đang tạm dừng bảo hành giày (LN Protect); theo thông lệ cửa hàng tại VN. Cần xác nhận.", "https://loansport.vn/ho-tro/chinh-sach-bao-hanh-giay-cau-long/56", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 30 },
                    { 8, 3, 6, "Đường may, khóa kéo (lỗi sản xuất)", "Chính sách chính thức LN Protect: túi 90 ngày kể từ ngày giao.", "https://in.lining.studio/pages/ln-protect-policy", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 90 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DefectTypes_CategoryId",
                table: "DefectTypes",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_DefectTypes_Code",
                table: "DefectTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyClaimHistory_ClaimId",
                table: "WarrantyClaimHistory",
                column: "ClaimId");

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyClaimMedia_ClaimId",
                table: "WarrantyClaimMedia",
                column: "ClaimId");

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyClaims_Code",
                table: "WarrantyClaims",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyClaims_DefectTypeId",
                table: "WarrantyClaims",
                column: "DefectTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyClaims_OrderId",
                table: "WarrantyClaims",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyClaims_OrderItemId",
                table: "WarrantyClaims",
                column: "OrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyClaims_Status_CreatedAt",
                table: "WarrantyClaims",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyClaims_UserId",
                table: "WarrantyClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyPolicies_BrandId_CategoryId",
                table: "WarrantyPolicies",
                columns: new[] { "BrandId", "CategoryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyPolicies_CategoryId",
                table: "WarrantyPolicies",
                column: "CategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WarrantyClaimHistory");

            migrationBuilder.DropTable(
                name: "WarrantyClaimMedia");

            migrationBuilder.DropTable(
                name: "WarrantyPolicies");

            migrationBuilder.DropTable(
                name: "WarrantyClaims");

            migrationBuilder.DropTable(
                name: "DefectTypes");

            migrationBuilder.DropColumn(
                name: "ReceiptDefectDays",
                table: "Brands");
        }
    }
}
