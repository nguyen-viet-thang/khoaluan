using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PopPop.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSampleUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "AvatarPath", "ConcurrencyStamp", "CreatedAt", "Email", "EmailConfirmed", "FullName", "LockReason", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[,]
                {
                    { "customer-001", 0, null, "demo-concurrency-001", new DateTime(2026, 2, 3, 9, 15, 0, 0, DateTimeKind.Utc), "an.nguyen@example.com", true, "Nguyễn Văn An", null, true, null, "AN.NGUYEN@EXAMPLE.COM", "AN.NGUYEN@EXAMPLE.COM", "AQAAAAIAAYagAAAAEOEUdp9KRxpQhrsF0xcWtmWJpjBiTeczdlnUVx+AAjLzZ89rzp0ciJBzX5wC1bwvUQ==", "0901234501", false, "DEMO-SECURITY-STAMP-001", false, "an.nguyen@example.com" },
                    { "customer-002", 0, null, "demo-concurrency-002", new DateTime(2026, 3, 11, 20, 40, 0, 0, DateTimeKind.Utc), "ngoc.tran@example.com", true, "Trần Thị Bích Ngọc", null, true, null, "NGOC.TRAN@EXAMPLE.COM", "NGOC.TRAN@EXAMPLE.COM", "AQAAAAIAAYagAAAAEOEUdp9KRxpQhrsF0xcWtmWJpjBiTeczdlnUVx+AAjLzZ89rzp0ciJBzX5wC1bwvUQ==", "0912345602", false, "DEMO-SECURITY-STAMP-002", false, "ngoc.tran@example.com" },
                    { "customer-003", 0, null, "demo-concurrency-003", new DateTime(2026, 4, 2, 13, 5, 0, 0, DateTimeKind.Utc), "minh.le@example.com", true, "Lê Hoàng Minh", null, true, null, "MINH.LE@EXAMPLE.COM", "MINH.LE@EXAMPLE.COM", "AQAAAAIAAYagAAAAEOEUdp9KRxpQhrsF0xcWtmWJpjBiTeczdlnUVx+AAjLzZ89rzp0ciJBzX5wC1bwvUQ==", "0987654303", false, "DEMO-SECURITY-STAMP-003", false, "minh.le@example.com" },
                    { "customer-004", 0, null, "demo-concurrency-004", new DateTime(2026, 5, 19, 8, 30, 0, 0, DateTimeKind.Utc), "ha.pham@example.com", true, "Phạm Thu Hà", null, true, null, "HA.PHAM@EXAMPLE.COM", "HA.PHAM@EXAMPLE.COM", "AQAAAAIAAYagAAAAEOEUdp9KRxpQhrsF0xcWtmWJpjBiTeczdlnUVx+AAjLzZ89rzp0ciJBzX5wC1bwvUQ==", "0934567804", false, "DEMO-SECURITY-STAMP-004", false, "ha.pham@example.com" },
                    { "customer-005", 0, null, "demo-concurrency-005", new DateTime(2026, 6, 7, 18, 55, 0, 0, DateTimeKind.Utc), "huy.hoang@example.com", true, "Hoàng Đức Huy", null, true, null, "HUY.HOANG@EXAMPLE.COM", "HUY.HOANG@EXAMPLE.COM", "AQAAAAIAAYagAAAAEOEUdp9KRxpQhrsF0xcWtmWJpjBiTeczdlnUVx+AAjLzZ89rzp0ciJBzX5wC1bwvUQ==", "0978123405", false, "DEMO-SECURITY-STAMP-005", false, "huy.hoang@example.com" },
                    { "customer-006", 0, null, "demo-concurrency-006", new DateTime(2026, 7, 23, 10, 20, 0, 0, DateTimeKind.Utc), "maianh.vu@example.com", true, "Vũ Thị Mai Anh", null, true, null, "MAIANH.VU@EXAMPLE.COM", "MAIANH.VU@EXAMPLE.COM", "AQAAAAIAAYagAAAAEOEUdp9KRxpQhrsF0xcWtmWJpjBiTeczdlnUVx+AAjLzZ89rzp0ciJBzX5wC1bwvUQ==", "0965432106", false, "DEMO-SECURITY-STAMP-006", false, "maianh.vu@example.com" },
                    { "customer-007", 0, null, "demo-concurrency-007", new DateTime(2026, 8, 30, 21, 10, 0, 0, DateTimeKind.Utc), "bao.dang@example.com", true, "Đặng Quốc Bảo", null, true, null, "BAO.DANG@EXAMPLE.COM", "BAO.DANG@EXAMPLE.COM", "AQAAAAIAAYagAAAAEOEUdp9KRxpQhrsF0xcWtmWJpjBiTeczdlnUVx+AAjLzZ89rzp0ciJBzX5wC1bwvUQ==", "0923456707", false, "DEMO-SECURITY-STAMP-007", false, "bao.dang@example.com" },
                    { "customer-008", 0, null, "demo-concurrency-008", new DateTime(2026, 10, 14, 7, 45, 0, 0, DateTimeKind.Utc), "tam.bui@example.com", false, "Bùi Thanh Tâm", null, true, null, "TAM.BUI@EXAMPLE.COM", "TAM.BUI@EXAMPLE.COM", "AQAAAAIAAYagAAAAEOEUdp9KRxpQhrsF0xcWtmWJpjBiTeczdlnUVx+AAjLzZ89rzp0ciJBzX5wC1bwvUQ==", "0945678908", false, "DEMO-SECURITY-STAMP-008", false, "tam.bui@example.com" },
                    { "customer-009", 0, null, "demo-concurrency-009", new DateTime(2026, 11, 5, 16, 0, 0, 0, DateTimeKind.Utc), "khoa.do@example.com", true, "Đỗ Minh Khoa", null, true, null, "KHOA.DO@EXAMPLE.COM", "KHOA.DO@EXAMPLE.COM", "AQAAAAIAAYagAAAAEOEUdp9KRxpQhrsF0xcWtmWJpjBiTeczdlnUVx+AAjLzZ89rzp0ciJBzX5wC1bwvUQ==", "0956789009", false, "DEMO-SECURITY-STAMP-009", false, "khoa.do@example.com" },
                    { "customer-010", 0, null, "demo-concurrency-010", new DateTime(2026, 9, 9, 12, 25, 0, 0, DateTimeKind.Utc), "han.ngo@example.com", true, "Ngô Gia Hân", "Nhiều lần từ chối nhận hàng COD (dữ liệu mẫu).", true, new DateTimeOffset(new DateTime(9999, 12, 31, 23, 59, 59, 999, DateTimeKind.Unspecified).AddTicks(9999), new TimeSpan(0, 0, 0, 0, 0)), "HAN.NGO@EXAMPLE.COM", "HAN.NGO@EXAMPLE.COM", "AQAAAAIAAYagAAAAEOEUdp9KRxpQhrsF0xcWtmWJpjBiTeczdlnUVx+AAjLzZ89rzp0ciJBzX5wC1bwvUQ==", "0909876510", false, "DEMO-SECURITY-STAMP-010", false, "han.ngo@example.com" },
                    { "staff-001", 0, null, "demo-concurrency-011", new DateTime(2026, 1, 15, 8, 0, 0, 0, DateTimeKind.Utc), "long.tran@example.com", true, "Trần Văn Long", null, true, null, "LONG.TRAN@EXAMPLE.COM", "LONG.TRAN@EXAMPLE.COM", "AQAAAAIAAYagAAAAEOEUdp9KRxpQhrsF0xcWtmWJpjBiTeczdlnUVx+AAjLzZ89rzp0ciJBzX5wC1bwvUQ==", "0911222301", false, "DEMO-SECURITY-STAMP-011", false, "long.tran@example.com" },
                    { "staff-002", 0, null, "demo-concurrency-012", new DateTime(2026, 3, 1, 8, 0, 0, 0, DateTimeKind.Utc), "lan.nguyen@example.com", true, "Nguyễn Thị Lan", null, true, null, "LAN.NGUYEN@EXAMPLE.COM", "LAN.NGUYEN@EXAMPLE.COM", "AQAAAAIAAYagAAAAEOEUdp9KRxpQhrsF0xcWtmWJpjBiTeczdlnUVx+AAjLzZ89rzp0ciJBzX5wC1bwvUQ==", "0911222302", false, "DEMO-SECURITY-STAMP-012", false, "lan.nguyen@example.com" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[,]
                {
                    { "3", "customer-001" },
                    { "3", "customer-002" },
                    { "3", "customer-003" },
                    { "3", "customer-004" },
                    { "3", "customer-005" },
                    { "3", "customer-006" },
                    { "3", "customer-007" },
                    { "3", "customer-008" },
                    { "3", "customer-009" },
                    { "3", "customer-010" },
                    { "2", "staff-001" },
                    { "2", "staff-002" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "3", "customer-001" });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "3", "customer-002" });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "3", "customer-003" });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "3", "customer-004" });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "3", "customer-005" });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "3", "customer-006" });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "3", "customer-007" });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "3", "customer-008" });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "3", "customer-009" });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "3", "customer-010" });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "2", "staff-001" });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "2", "staff-002" });

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "customer-001");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "customer-002");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "customer-003");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "customer-004");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "customer-005");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "customer-006");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "customer-007");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "customer-008");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "customer-009");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "customer-010");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "staff-001");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "staff-002");
        }
    }
}
