using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PopPop.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProductSpecifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SpecAttributes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Scope = table.Column<int>(type: "int", nullable: false),
                    Hint = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecAttributes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpecAttributes_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpecOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SpecAttributeId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Hint = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpecOptions_SpecAttributes_SpecAttributeId",
                        column: x => x.SpecAttributeId,
                        principalTable: "SpecAttributes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductSpecValues",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    SpecOptionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductSpecValues", x => new { x.ProductId, x.SpecOptionId });
                    table.ForeignKey(
                        name: "FK_ProductSpecValues_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductSpecValues_SpecOptions_SpecOptionId",
                        column: x => x.SpecOptionId,
                        principalTable: "SpecOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VariantSpecValues",
                columns: table => new
                {
                    ProductVariantId = table.Column<int>(type: "int", nullable: false),
                    SpecOptionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VariantSpecValues", x => new { x.ProductVariantId, x.SpecOptionId });
                    table.ForeignKey(
                        name: "FK_VariantSpecValues_ProductVariants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VariantSpecValues_SpecOptions_SpecOptionId",
                        column: x => x.SpecOptionId,
                        principalTable: "SpecOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "SpecAttributes",
                columns: new[] { "Id", "CategoryId", "Hint", "Name", "Scope", "SortOrder" },
                values: new object[,]
                {
                    { 1, 1, "Ký hiệu U càng lớn vợt càng nhẹ.", "Trọng lượng", 2, 1 },
                    { 2, 1, "Số G càng lớn cán càng nhỏ.", "Cỡ cán", 2, 2 },
                    { 3, 1, "Quyết định cảm giác vung và lối chơi phù hợp.", "Điểm cân bằng", 1, 3 },
                    { 4, 1, "Thân dẻo dễ trợ lực; thân cứng cần lực cổ tay tốt nhưng kiểm soát chính xác hơn.", "Độ cứng thân", 1, 4 },
                    { 5, 2, null, "Size", 2, 5 },
                    { 6, 2, null, "Màu", 1, 6 },
                    { 7, 3, null, "Dành cho", 1, 7 },
                    { 8, 3, null, "Size", 2, 8 },
                    { 9, 3, null, "Màu", 1, 9 },
                    { 10, 4, null, "Loại", 1, 10 },
                    { 11, 4, null, "Dành cho", 1, 11 },
                    { 12, 4, null, "Size", 2, 12 },
                    { 13, 4, null, "Màu", 1, 13 },
                    { 14, 5, "Chọn theo nhiệt độ và độ cao của sân: sân nóng chọn số nhỏ, sân lạnh chọn số lớn.", "Tốc độ", 2, 14 },
                    { 15, 6, null, "Loại", 1, 15 },
                    { 16, 6, null, "Màu", 2, 16 },
                    { 17, 7, "Cước mảnh nảy hơn, đanh tiếng; cước dày bền hơn.", "Đường kính", 1, 17 },
                    { 18, 7, null, "Màu", 2, 18 },
                    { 19, 8, null, "Loại", 1, 19 },
                    { 20, 8, null, "Màu", 2, 20 },
                    { 21, 9, null, "Loại", 1, 21 }
                });

            migrationBuilder.InsertData(
                table: "SpecOptions",
                columns: new[] { "Id", "Hint", "SortOrder", "SpecAttributeId", "Value" },
                values: new object[,]
                {
                    { 1, "85–89 g", 1, 1, "3U" },
                    { 2, "80–84 g", 2, 1, "4U" },
                    { 3, "55–59 g", 3, 1, "8U" },
                    { 4, null, 1, 2, "G5" },
                    { 5, null, 2, 2, "G6" },
                    { 6, "Thiên tấn công, đập cầu uy lực", 1, 3, "Nặng đầu" },
                    { 7, "Công thủ toàn diện", 2, 3, "Cân bằng" },
                    { 8, "Phòng thủ, phản tạt nhanh", 3, 3, "Nhẹ đầu" },
                    { 9, "Dễ chơi, hợp người mới", 1, 4, "Dẻo" },
                    { 10, "Phù hợp đa số người chơi", 2, 4, "Trung bình" },
                    { 11, "Cho người chơi có lực cổ tay tốt", 3, 4, "Cứng" },
                    { 12, null, 1, 5, "38" },
                    { 13, null, 2, 5, "39" },
                    { 14, null, 3, 5, "40" },
                    { 15, null, 4, 5, "41" },
                    { 16, null, 5, 5, "42" },
                    { 17, null, 6, 5, "43" },
                    { 18, null, 1, 6, "Trắng" },
                    { 19, null, 2, 6, "Đen" },
                    { 20, null, 1, 7, "Nam" },
                    { 21, null, 2, 7, "Nữ" },
                    { 22, null, 1, 8, "S" },
                    { 23, null, 2, 8, "M" },
                    { 24, null, 3, 8, "L" },
                    { 25, null, 4, 8, "XL" },
                    { 26, null, 1, 9, "Trắng" },
                    { 27, null, 2, 9, "Đen" },
                    { 28, null, 3, 9, "Đỏ" },
                    { 29, null, 4, 9, "Xanh navy" },
                    { 30, null, 5, 9, "Xanh dương" },
                    { 31, null, 1, 10, "Quần short" },
                    { 32, null, 2, 10, "Váy" },
                    { 33, null, 1, 11, "Nam" },
                    { 34, null, 2, 11, "Nữ" },
                    { 35, null, 1, 12, "S" },
                    { 36, null, 2, 12, "M" },
                    { 37, null, 3, 12, "L" },
                    { 38, null, 4, 12, "XL" },
                    { 39, null, 1, 13, "Đen" },
                    { 40, null, 2, 13, "Xanh navy" },
                    { 41, null, 1, 14, "76" },
                    { 42, null, 2, 14, "77" },
                    { 43, null, 1, 15, "Túi vợt" },
                    { 44, null, 2, 15, "Balo" },
                    { 45, null, 1, 16, "Nâu" },
                    { 46, null, 2, 16, "Đen" },
                    { 47, null, 3, 16, "Hồng" },
                    { 48, null, 4, 16, "Xanh navy" },
                    { 49, null, 1, 17, "0,63 mm" },
                    { 50, null, 2, 17, "0,65 mm" },
                    { 51, null, 3, 17, "0,66 mm" },
                    { 52, null, 4, 17, "0,68 mm" },
                    { 53, null, 5, 17, "0,70 mm" },
                    { 54, null, 1, 18, "Trắng" },
                    { 55, null, 2, 18, "Vàng" },
                    { 56, "Bám tay, mỏng", 1, 19, "Cao su (PU)" },
                    { 57, "Thấm mồ hôi tốt", 2, 19, "Khăn" },
                    { 58, null, 1, 20, "Trắng" },
                    { 59, null, 2, 20, "Đen" },
                    { 60, null, 3, 20, "Vàng" },
                    { 61, null, 4, 20, "Đỏ" },
                    { 62, null, 1, 21, "Tất" },
                    { 63, null, 2, 21, "Băng cổ tay" },
                    { 64, null, 3, 21, "Lưới" }
                });

            migrationBuilder.InsertData(
                table: "ProductSpecValues",
                columns: new[] { "ProductId", "SpecOptionId" },
                values: new object[,]
                {
                    { 1, 6 },
                    { 1, 11 },
                    { 2, 6 },
                    { 2, 11 },
                    { 3, 8 },
                    { 3, 11 },
                    { 4, 7 },
                    { 4, 11 },
                    { 5, 8 },
                    { 5, 11 },
                    { 6, 6 },
                    { 6, 10 },
                    { 7, 8 },
                    { 7, 10 },
                    { 8, 6 },
                    { 8, 11 },
                    { 9, 8 },
                    { 9, 11 },
                    { 10, 7 },
                    { 10, 10 },
                    { 11, 6 },
                    { 11, 10 },
                    { 12, 6 },
                    { 12, 10 },
                    { 13, 7 },
                    { 13, 11 },
                    { 14, 6 },
                    { 14, 9 },
                    { 15, 18 },
                    { 16, 18 },
                    { 17, 19 },
                    { 18, 19 },
                    { 19, 18 },
                    { 20, 20 },
                    { 20, 29 },
                    { 21, 21 },
                    { 21, 26 },
                    { 22, 20 },
                    { 22, 28 },
                    { 23, 20 },
                    { 23, 27 },
                    { 24, 20 },
                    { 24, 30 },
                    { 25, 31 },
                    { 25, 33 },
                    { 25, 39 },
                    { 26, 32 },
                    { 26, 34 },
                    { 26, 40 },
                    { 27, 31 },
                    { 27, 33 },
                    { 27, 39 },
                    { 32, 43 },
                    { 33, 44 },
                    { 34, 43 },
                    { 35, 53 },
                    { 36, 50 },
                    { 37, 52 },
                    { 38, 49 },
                    { 39, 51 },
                    { 40, 50 },
                    { 41, 56 },
                    { 42, 56 },
                    { 43, 57 },
                    { 44, 62 },
                    { 45, 63 },
                    { 46, 64 }
                });

            migrationBuilder.InsertData(
                table: "VariantSpecValues",
                columns: new[] { "ProductVariantId", "SpecOptionId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 1, 4 },
                    { 2, 2 },
                    { 2, 4 },
                    { 3, 1 },
                    { 3, 4 },
                    { 4, 2 },
                    { 4, 4 },
                    { 5, 2 },
                    { 5, 4 },
                    { 6, 2 },
                    { 6, 5 },
                    { 7, 1 },
                    { 7, 4 },
                    { 8, 2 },
                    { 8, 4 },
                    { 9, 2 },
                    { 9, 4 },
                    { 10, 1 },
                    { 10, 4 },
                    { 11, 2 },
                    { 11, 4 },
                    { 12, 2 },
                    { 12, 4 },
                    { 13, 1 },
                    { 13, 4 },
                    { 14, 2 },
                    { 14, 4 },
                    { 15, 2 },
                    { 15, 4 },
                    { 16, 1 },
                    { 16, 4 },
                    { 17, 2 },
                    { 17, 4 },
                    { 18, 1 },
                    { 18, 4 },
                    { 19, 2 },
                    { 19, 4 },
                    { 20, 2 },
                    { 20, 4 },
                    { 21, 1 },
                    { 21, 4 },
                    { 22, 2 },
                    { 22, 4 },
                    { 23, 3 },
                    { 23, 4 },
                    { 24, 13 },
                    { 25, 14 },
                    { 26, 15 },
                    { 27, 16 },
                    { 28, 17 },
                    { 29, 14 },
                    { 30, 15 },
                    { 31, 16 },
                    { 32, 17 },
                    { 33, 13 },
                    { 34, 14 },
                    { 35, 15 },
                    { 36, 16 },
                    { 37, 17 },
                    { 38, 13 },
                    { 39, 14 },
                    { 40, 15 },
                    { 41, 16 },
                    { 42, 17 },
                    { 43, 12 },
                    { 44, 13 },
                    { 45, 14 },
                    { 46, 15 },
                    { 47, 16 },
                    { 48, 23 },
                    { 49, 24 },
                    { 50, 25 },
                    { 51, 22 },
                    { 52, 23 },
                    { 53, 24 },
                    { 54, 23 },
                    { 55, 24 },
                    { 56, 25 },
                    { 57, 23 },
                    { 58, 24 },
                    { 59, 25 },
                    { 60, 23 },
                    { 61, 24 },
                    { 62, 25 },
                    { 63, 36 },
                    { 64, 37 },
                    { 65, 38 },
                    { 66, 35 },
                    { 67, 36 },
                    { 68, 37 },
                    { 69, 36 },
                    { 70, 37 },
                    { 71, 38 },
                    { 72, 41 },
                    { 73, 42 },
                    { 74, 41 },
                    { 75, 42 },
                    { 76, 42 },
                    { 77, 42 },
                    { 78, 45 },
                    { 79, 46 },
                    { 80, 47 },
                    { 81, 48 },
                    { 82, 54 },
                    { 83, 55 },
                    { 84, 54 },
                    { 85, 55 },
                    { 86, 54 },
                    { 87, 55 },
                    { 88, 54 },
                    { 89, 55 },
                    { 90, 54 },
                    { 91, 54 },
                    { 92, 58 },
                    { 93, 59 },
                    { 94, 60 },
                    { 95, 61 },
                    { 96, 59 },
                    { 97, 59 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductSpecValues_SpecOptionId",
                table: "ProductSpecValues",
                column: "SpecOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_SpecAttributes_CategoryId_Name",
                table: "SpecAttributes",
                columns: new[] { "CategoryId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SpecOptions_SpecAttributeId_Value",
                table: "SpecOptions",
                columns: new[] { "SpecAttributeId", "Value" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VariantSpecValues_SpecOptionId",
                table: "VariantSpecValues",
                column: "SpecOptionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductSpecValues");

            migrationBuilder.DropTable(
                name: "VariantSpecValues");

            migrationBuilder.DropTable(
                name: "SpecOptions");

            migrationBuilder.DropTable(
                name: "SpecAttributes");
        }
    }
}
