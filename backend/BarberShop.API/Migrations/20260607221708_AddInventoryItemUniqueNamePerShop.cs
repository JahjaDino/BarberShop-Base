using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarberShop.API.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryItemUniqueNamePerShop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InventoryItems_ShopId",
                table: "InventoryItems");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_ShopId_Name",
                table: "InventoryItems",
                columns: new[] { "ShopId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InventoryItems_ShopId_Name",
                table: "InventoryItems");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_ShopId",
                table: "InventoryItems",
                column: "ShopId");
        }
    }
}
