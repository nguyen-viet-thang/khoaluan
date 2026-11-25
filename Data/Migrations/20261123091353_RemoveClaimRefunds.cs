using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PopPop.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveClaimRefunds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RefundAccountHolder",
                table: "WarrantyClaims");

            migrationBuilder.DropColumn(
                name: "RefundAccountNumber",
                table: "WarrantyClaims");

            migrationBuilder.DropColumn(
                name: "RefundAmount",
                table: "WarrantyClaims");

            migrationBuilder.DropColumn(
                name: "RefundBankName",
                table: "WarrantyClaims");

            migrationBuilder.DropColumn(
                name: "RefundReference",
                table: "WarrantyClaims");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RefundAccountHolder",
                table: "WarrantyClaims",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RefundAccountNumber",
                table: "WarrantyClaims",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RefundAmount",
                table: "WarrantyClaims",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RefundBankName",
                table: "WarrantyClaims",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RefundReference",
                table: "WarrantyClaims",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
