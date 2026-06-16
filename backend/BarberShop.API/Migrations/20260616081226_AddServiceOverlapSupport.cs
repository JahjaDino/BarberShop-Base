using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarberShop.API.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceOverlapSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowOverlap",
                table: "Services",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaxParallelAppointments",
                table: "Services",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Services_MaxParallelAppointments_Range",
                table: "Services",
                sql: "\"MaxParallelAppointments\" >= 1 AND \"MaxParallelAppointments\" <= 3");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Services_MaxParallelAppointments_Range",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "AllowOverlap",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "MaxParallelAppointments",
                table: "Services");
        }
    }
}
