using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarberShop.API.Migrations
{
    /// <inheritdoc />
    public partial class AddDatabaseConstraintsAndUniqueIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserRoles_UserId",
                table: "UserRoles");

            migrationBuilder.DropIndex(
                name: "IX_TimeOffs_EmployeeId_StartTime_EndTime",
                table: "TimeOffs");

            migrationBuilder.DropIndex(
                name: "IX_Employees_UserId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Clients_UserId",
                table: "Clients");

            migrationBuilder.CreateIndex(
                name: "IX_WorkingHours_EmployeeId_DayOfWeek_StartTime_EndTime",
                table: "WorkingHours",
                columns: new[] { "EmployeeId", "DayOfWeek", "StartTime", "EndTime" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_WorkingHours_StartBeforeEnd",
                table: "WorkingHours",
                sql: "\"StartTime\" < \"EndTime\"");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId_RoleId",
                table: "UserRoles",
                columns: new[] { "UserId", "RoleId" },
                unique: true,
                filter: "\"ShopId\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId_RoleId_ShopId",
                table: "UserRoles",
                columns: new[] { "UserId", "RoleId", "ShopId" },
                unique: true,
                filter: "\"ShopId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TimeOffs_EmployeeId_StartTime_EndTime",
                table: "TimeOffs",
                columns: new[] { "EmployeeId", "StartTime", "EndTime" },
                unique: true,
                filter: "\"Status\" NOT IN ('CANCELLED', 'REJECTED')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_TimeOffs_StartBeforeEnd",
                table: "TimeOffs",
                sql: "\"StartTime\" < \"EndTime\"");

            migrationBuilder.CreateIndex(
                name: "IX_Services_ShopId_CategoryId_Name",
                table: "Services",
                columns: new[] { "ShopId", "CategoryId", "Name" },
                unique: true,
                filter: "\"Active\" = TRUE");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Services_DurationMinutes_Positive",
                table: "Services",
                sql: "\"DurationMinutes\" > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Services_Price_NonNegative",
                table: "Services",
                sql: "\"Price\" >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCategories_ShopId_Name",
                table: "ServiceCategories",
                columns: new[] { "ShopId", "Name" },
                unique: true,
                filter: "\"Active\" = TRUE");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name",
                table: "Roles",
                column: "Name",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Reviews_Rating_Range",
                table: "Reviews",
                sql: "\"Rating\" BETWEEN 1 AND 5");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Payments_Amount_NonNegative",
                table: "Payments",
                sql: "\"Amount\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_InventoryItems_MinimumQuantity_NonNegative",
                table: "InventoryItems",
                sql: "\"MinimumQuantity\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_InventoryItems_Quantity_NonNegative",
                table: "InventoryItems",
                sql: "\"Quantity\" >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_ShopId_UserId",
                table: "Employees",
                columns: new[] { "ShopId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_UserId",
                table: "Employees",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clients_UserId",
                table: "Clients",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WorkingHours_EmployeeId_DayOfWeek_StartTime_EndTime",
                table: "WorkingHours");

            migrationBuilder.DropCheckConstraint(
                name: "CK_WorkingHours_StartBeforeEnd",
                table: "WorkingHours");

            migrationBuilder.DropIndex(
                name: "IX_UserRoles_UserId_RoleId",
                table: "UserRoles");

            migrationBuilder.DropIndex(
                name: "IX_UserRoles_UserId_RoleId_ShopId",
                table: "UserRoles");

            migrationBuilder.DropIndex(
                name: "IX_TimeOffs_EmployeeId_StartTime_EndTime",
                table: "TimeOffs");

            migrationBuilder.DropCheckConstraint(
                name: "CK_TimeOffs_StartBeforeEnd",
                table: "TimeOffs");

            migrationBuilder.DropIndex(
                name: "IX_Services_ShopId_CategoryId_Name",
                table: "Services");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Services_DurationMinutes_Positive",
                table: "Services");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Services_Price_NonNegative",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_ServiceCategories_ShopId_Name",
                table: "ServiceCategories");

            migrationBuilder.DropIndex(
                name: "IX_Roles_Name",
                table: "Roles");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Reviews_Rating_Range",
                table: "Reviews");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Payments_Amount_NonNegative",
                table: "Payments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_InventoryItems_MinimumQuantity_NonNegative",
                table: "InventoryItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_InventoryItems_Quantity_NonNegative",
                table: "InventoryItems");

            migrationBuilder.DropIndex(
                name: "IX_Employees_ShopId_UserId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_UserId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Clients_UserId",
                table: "Clients");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId",
                table: "UserRoles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeOffs_EmployeeId_StartTime_EndTime",
                table: "TimeOffs",
                columns: new[] { "EmployeeId", "StartTime", "EndTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Employees_UserId",
                table: "Employees",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_UserId",
                table: "Clients",
                column: "UserId");
        }
    }
}
