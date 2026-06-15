using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarberShop.API.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryReportMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReportNote",
                table: "InventoryItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReportedAt",
                table: "InventoryItems",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReportedByEmployeeId",
                table: "InventoryItems",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_ReportedByEmployeeId",
                table: "InventoryItems",
                column: "ReportedByEmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryItems_Employees_ReportedByEmployeeId",
                table: "InventoryItems",
                column: "ReportedByEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryItems_Employees_ReportedByEmployeeId",
                table: "InventoryItems");

            migrationBuilder.DropIndex(
                name: "IX_InventoryItems_ReportedByEmployeeId",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "ReportNote",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "ReportedAt",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "ReportedByEmployeeId",
                table: "InventoryItems");
        }
    }
}
