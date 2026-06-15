using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarberShop.API.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeOffReviewAuditFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReviewNote",
                table: "TimeOffs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAt",
                table: "TimeOffs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReviewedByUserId",
                table: "TimeOffs",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TimeOffs_ReviewedByUserId",
                table: "TimeOffs",
                column: "ReviewedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TimeOffs_Users_ReviewedByUserId",
                table: "TimeOffs",
                column: "ReviewedByUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TimeOffs_Users_ReviewedByUserId",
                table: "TimeOffs");

            migrationBuilder.DropIndex(
                name: "IX_TimeOffs_ReviewedByUserId",
                table: "TimeOffs");

            migrationBuilder.DropColumn(
                name: "ReviewNote",
                table: "TimeOffs");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                table: "TimeOffs");

            migrationBuilder.DropColumn(
                name: "ReviewedByUserId",
                table: "TimeOffs");
        }
    }
}
