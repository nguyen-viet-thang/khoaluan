using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PopPop.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddClaimShippingAndRefund : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OutboundCarrier",
                table: "WarrantyClaims",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OutboundShippedAt",
                table: "WarrantyClaims",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OutboundTrackingCode",
                table: "WarrantyClaims",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

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

            migrationBuilder.AddColumn<string>(
                name: "ReturnCarrier",
                table: "WarrantyClaims",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReturnShippedAt",
                table: "WarrantyClaims",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnTrackingCode",
                table: "WarrantyClaims",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OutboundCarrier",
                table: "WarrantyClaims");

            migrationBuilder.DropColumn(
                name: "OutboundShippedAt",
                table: "WarrantyClaims");

            migrationBuilder.DropColumn(
                name: "OutboundTrackingCode",
                table: "WarrantyClaims");

            migrationBuilder.DropColumn(
                name: "RefundAccountHolder",
                table: "WarrantyClaims");

            migrationBuilder.DropColumn(
                name: "RefundAccountNumber",
                table: "WarrantyClaims");

            migrationBuilder.DropColumn(
                name: "RefundBankName",
                table: "WarrantyClaims");

            migrationBuilder.DropColumn(
                name: "RefundReference",
                table: "WarrantyClaims");

            migrationBuilder.DropColumn(
                name: "ReturnCarrier",
                table: "WarrantyClaims");

            migrationBuilder.DropColumn(
                name: "ReturnShippedAt",
                table: "WarrantyClaims");

            migrationBuilder.DropColumn(
                name: "ReturnTrackingCode",
                table: "WarrantyClaims");
        }
    }
}
