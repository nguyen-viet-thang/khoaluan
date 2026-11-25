using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PopPop.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveEmailUsePhoneLogin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "EmailIndex",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "NormalizedEmail",
                table: "AspNetUsers");

            // Not a rename: EmailConfirmed values must NOT become "must change password".
            migrationBuilder.DropColumn(
                name: "EmailConfirmed",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<bool>(
                name: "MustChangePassword",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-001",
                columns: new[] { "MustChangePassword", "NormalizedUserName", "PhoneNumber", "UserName" },
                values: new object[] { false, "0900000000", "0900000000", "0900000000" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "customer-001",
                columns: new[] { "MustChangePassword", "NormalizedUserName", "UserName" },
                values: new object[] { false, "0901234501", "0901234501" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "customer-002",
                columns: new[] { "MustChangePassword", "NormalizedUserName", "UserName" },
                values: new object[] { false, "0912345602", "0912345602" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "customer-003",
                columns: new[] { "MustChangePassword", "NormalizedUserName", "UserName" },
                values: new object[] { false, "0987654303", "0987654303" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "customer-004",
                columns: new[] { "MustChangePassword", "NormalizedUserName", "UserName" },
                values: new object[] { false, "0934567804", "0934567804" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "customer-005",
                columns: new[] { "MustChangePassword", "NormalizedUserName", "UserName" },
                values: new object[] { false, "0978123405", "0978123405" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "customer-006",
                columns: new[] { "MustChangePassword", "NormalizedUserName", "UserName" },
                values: new object[] { false, "0965432106", "0965432106" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "customer-007",
                columns: new[] { "MustChangePassword", "NormalizedUserName", "UserName" },
                values: new object[] { false, "0923456707", "0923456707" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "customer-008",
                columns: new[] { "NormalizedUserName", "UserName" },
                values: new object[] { "0945678908", "0945678908" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "customer-009",
                columns: new[] { "MustChangePassword", "NormalizedUserName", "UserName" },
                values: new object[] { false, "0956789009", "0956789009" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "customer-010",
                columns: new[] { "MustChangePassword", "NormalizedUserName", "UserName" },
                values: new object[] { false, "0909876510", "0909876510" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "staff-001",
                columns: new[] { "MustChangePassword", "NormalizedUserName", "UserName" },
                values: new object[] { false, "0911222301", "0911222301" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "staff-002",
                columns: new[] { "MustChangePassword", "NormalizedUserName", "UserName" },
                values: new object[] { false, "0911222302", "0911222302" });

            // Existing (non-seeded) accounts: sign in with their phone number from now on when it is a valid,
            // unused 10-digit number. Accounts without one keep their old user name until an admin/the user sets it.
            migrationBuilder.Sql(@"
UPDATE u SET UserName = u.PhoneNumber, NormalizedUserName = u.PhoneNumber
FROM AspNetUsers u
WHERE u.PhoneNumber LIKE '0[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'
  AND u.UserName <> u.PhoneNumber
  AND NOT EXISTS (SELECT 1 FROM AspNetUsers other WHERE other.Id <> u.Id
                  AND (other.NormalizedUserName = u.PhoneNumber OR other.PhoneNumber = u.PhoneNumber));");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MustChangePassword",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<bool>(
                name: "EmailConfirmed",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "AspNetUsers",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedEmail",
                table: "AspNetUsers",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-001",
                columns: new[] { "Email", "EmailConfirmed", "NormalizedEmail", "NormalizedUserName", "PhoneNumber", "UserName" },
                values: new object[] { "admin@poppop.com", true, "ADMIN@POPPOP.COM", "ADMIN@POPPOP.COM", null, "admin@poppop.com" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "customer-001",
                columns: new[] { "Email", "EmailConfirmed", "NormalizedEmail", "NormalizedUserName", "UserName" },
                values: new object[] { "an.nguyen@example.com", true, "AN.NGUYEN@EXAMPLE.COM", "AN.NGUYEN@EXAMPLE.COM", "an.nguyen@example.com" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "customer-002",
                columns: new[] { "Email", "EmailConfirmed", "NormalizedEmail", "NormalizedUserName", "UserName" },
                values: new object[] { "ngoc.tran@example.com", true, "NGOC.TRAN@EXAMPLE.COM", "NGOC.TRAN@EXAMPLE.COM", "ngoc.tran@example.com" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "customer-003",
                columns: new[] { "Email", "EmailConfirmed", "NormalizedEmail", "NormalizedUserName", "UserName" },
                values: new object[] { "minh.le@example.com", true, "MINH.LE@EXAMPLE.COM", "MINH.LE@EXAMPLE.COM", "minh.le@example.com" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "customer-004",
                columns: new[] { "Email", "EmailConfirmed", "NormalizedEmail", "NormalizedUserName", "UserName" },
                values: new object[] { "ha.pham@example.com", true, "HA.PHAM@EXAMPLE.COM", "HA.PHAM@EXAMPLE.COM", "ha.pham@example.com" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "customer-005",
                columns: new[] { "Email", "EmailConfirmed", "NormalizedEmail", "NormalizedUserName", "UserName" },
                values: new object[] { "huy.hoang@example.com", true, "HUY.HOANG@EXAMPLE.COM", "HUY.HOANG@EXAMPLE.COM", "huy.hoang@example.com" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "customer-006",
                columns: new[] { "Email", "EmailConfirmed", "NormalizedEmail", "NormalizedUserName", "UserName" },
                values: new object[] { "maianh.vu@example.com", true, "MAIANH.VU@EXAMPLE.COM", "MAIANH.VU@EXAMPLE.COM", "maianh.vu@example.com" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "customer-007",
                columns: new[] { "Email", "EmailConfirmed", "NormalizedEmail", "NormalizedUserName", "UserName" },
                values: new object[] { "bao.dang@example.com", true, "BAO.DANG@EXAMPLE.COM", "BAO.DANG@EXAMPLE.COM", "bao.dang@example.com" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "customer-008",
                columns: new[] { "Email", "NormalizedEmail", "NormalizedUserName", "UserName" },
                values: new object[] { "tam.bui@example.com", "TAM.BUI@EXAMPLE.COM", "TAM.BUI@EXAMPLE.COM", "tam.bui@example.com" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "customer-009",
                columns: new[] { "Email", "EmailConfirmed", "NormalizedEmail", "NormalizedUserName", "UserName" },
                values: new object[] { "khoa.do@example.com", true, "KHOA.DO@EXAMPLE.COM", "KHOA.DO@EXAMPLE.COM", "khoa.do@example.com" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "customer-010",
                columns: new[] { "Email", "EmailConfirmed", "NormalizedEmail", "NormalizedUserName", "UserName" },
                values: new object[] { "han.ngo@example.com", true, "HAN.NGO@EXAMPLE.COM", "HAN.NGO@EXAMPLE.COM", "han.ngo@example.com" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "staff-001",
                columns: new[] { "Email", "EmailConfirmed", "NormalizedEmail", "NormalizedUserName", "UserName" },
                values: new object[] { "long.tran@example.com", true, "LONG.TRAN@EXAMPLE.COM", "LONG.TRAN@EXAMPLE.COM", "long.tran@example.com" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "staff-002",
                columns: new[] { "Email", "EmailConfirmed", "NormalizedEmail", "NormalizedUserName", "UserName" },
                values: new object[] { "lan.nguyen@example.com", true, "LAN.NGUYEN@EXAMPLE.COM", "LAN.NGUYEN@EXAMPLE.COM", "lan.nguyen@example.com" });

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");
        }
    }
}
