using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarberShop.API.Migrations
{
    /// <inheritdoc />
    public partial class AddAppointmentIndexesAndConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_AppointmentServices_DurationAtBooking_Positive",
                table: "AppointmentServices",
                sql: "\"DurationAtBooking\" > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AppointmentServices_PriceAtBooking_NonNegative",
                table: "AppointmentServices",
                sql: "\"PriceAtBooking\" >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_EmployeeId_StartTime_EndTime",
                table: "Appointments",
                columns: new[] { "EmployeeId", "StartTime", "EndTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_Status",
                table: "Appointments",
                column: "Status");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Appointments_StartBeforeEnd",
                table: "Appointments",
                sql: "\"StartTime\" < \"EndTime\"");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Appointments_TotalPrice_NonNegative",
                table: "Appointments",
                sql: "\"TotalPrice\" >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_AppointmentServices_DurationAtBooking_Positive",
                table: "AppointmentServices");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AppointmentServices_PriceAtBooking_NonNegative",
                table: "AppointmentServices");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_EmployeeId_StartTime_EndTime",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_Status",
                table: "Appointments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Appointments_StartBeforeEnd",
                table: "Appointments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Appointments_TotalPrice_NonNegative",
                table: "Appointments");
        }
    }
}
