using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PopPop.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerEmailLogin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "AspNetUsers",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EmailConfirmed",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

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
                columns: new[] { "Email", "EmailConfirmed", "NormalizedEmail" },
                values: new object[] { null, false, null });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "customer-001",
                columns: new[] { "Email", "EmailConfirmed", "NormalizedEmail" },
                values: new object[] { null, false, null });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "customer-002",
                columns: new[] { "Email", "EmailConfirmed", "NormalizedEmail" },
                values: new object[] { null, false, null });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "customer-003",
                columns: new[] { "Email", "EmailConfirmed", "NormalizedEmail" },
                values: new object[] { null, false, null });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "customer-004",
                columns: new[] { "Email", "EmailConfirmed", "NormalizedEmail" },
                values: new object[] { null, false, null });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "customer-005",
                columns: new[] { "Email", "EmailConfirmed", "NormalizedEmail" },
                values: new object[] { null, false, null });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "customer-006",
                columns: new[] { "Email", "EmailConfirmed", "NormalizedEmail" },
                values: new object[] { null, false, null });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "customer-007",
                columns: new[] { "Email", "EmailConfirmed", "NormalizedEmail" },
                values: new object[] { null, false, null });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "customer-008",
                columns: new[] { "Email", "EmailConfirmed", "NormalizedEmail" },
                values: new object[] { null, false, null });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "customer-009",
                columns: new[] { "Email", "EmailConfirmed", "NormalizedEmail" },
                values: new object[] { null, false, null });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "customer-010",
                columns: new[] { "Email", "EmailConfirmed", "NormalizedEmail" },
                values: new object[] { null, false, null });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "staff-001",
                columns: new[] { "Email", "EmailConfirmed", "NormalizedEmail" },
                values: new object[] { null, false, null });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "staff-002",
                columns: new[] { "Email", "EmailConfirmed", "NormalizedEmail" },
                values: new object[] { null, false, null });

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "EmailIndex",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "EmailConfirmed",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "NormalizedEmail",
                table: "AspNetUsers");
        }
    }
}
