using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarberShop.API.Migrations
{
    /// <inheritdoc />
    public partial class AddShopRelationToUserRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_ShopId",
                table: "UserRoles",
                column: "ShopId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_Shops_ShopId",
                table: "UserRoles",
                column: "ShopId",
                principalTable: "Shops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_Shops_ShopId",
                table: "UserRoles");

            migrationBuilder.DropIndex(
                name: "IX_UserRoles_ShopId",
                table: "UserRoles");
        }
    }
}
