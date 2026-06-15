using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarberShop.API.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_WorkingHours_EmployeeId_DayOfWeek",
                table: "WorkingHours",
                columns: new[] { "EmployeeId", "DayOfWeek" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkingHours_EmployeeId_DayOfWeek_Active",
                table: "WorkingHours",
                columns: new[] { "EmployeeId", "DayOfWeek", "Active" });

            migrationBuilder.CreateIndex(
                name: "IX_TimeOffs_EmployeeId_StartTime_EndTime",
                table: "TimeOffs",
                columns: new[] { "EmployeeId", "StartTime", "EndTime" });

            migrationBuilder.CreateIndex(
                name: "IX_TimeOffs_EmployeeId_Status",
                table: "TimeOffs",
                columns: new[] { "EmployeeId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Services_ShopId_Active",
                table: "Services",
                columns: new[] { "ShopId", "Active" });

            migrationBuilder.CreateIndex(
                name: "IX_Services_ShopId_CategoryId_Active",
                table: "Services",
                columns: new[] { "ShopId", "CategoryId", "Active" });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCategories_ShopId_Active",
                table: "ServiceCategories",
                columns: new[] { "ShopId", "Active" });

            migrationBuilder.CreateIndex(
                name: "IX_Employees_ShopId_Active",
                table: "Employees",
                columns: new[] { "ShopId", "Active" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WorkingHours_EmployeeId_DayOfWeek",
                table: "WorkingHours");

            migrationBuilder.DropIndex(
                name: "IX_WorkingHours_EmployeeId_DayOfWeek_Active",
                table: "WorkingHours");

            migrationBuilder.DropIndex(
                name: "IX_TimeOffs_EmployeeId_StartTime_EndTime",
                table: "TimeOffs");

            migrationBuilder.DropIndex(
                name: "IX_TimeOffs_EmployeeId_Status",
                table: "TimeOffs");

            migrationBuilder.DropIndex(
                name: "IX_Services_ShopId_Active",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_Services_ShopId_CategoryId_Active",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_ServiceCategories_ShopId_Active",
                table: "ServiceCategories");

            migrationBuilder.DropIndex(
                name: "IX_Employees_ShopId_Active",
                table: "Employees");
        }
    }
}
