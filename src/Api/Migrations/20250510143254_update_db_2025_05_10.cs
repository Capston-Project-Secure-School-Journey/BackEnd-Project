using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class update_db_2025_05_10 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "pick_up_lng",
                table: "students",
                type: "double",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,6)");

            migrationBuilder.AlterColumn<double>(
                name: "pick_up_lat",
                table: "students",
                type: "double",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,6)");

            migrationBuilder.CreateIndex(
                name: "IX_driver_approval_requests_driver_id",
                table: "driver_approval_requests",
                column: "driver_id");

            migrationBuilder.AddForeignKey(
                name: "FK_driver_approval_requests_users_driver_id",
                table: "driver_approval_requests",
                column: "driver_id",
                principalTable: "users",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_driver_approval_requests_users_driver_id",
                table: "driver_approval_requests");

            migrationBuilder.DropIndex(
                name: "IX_driver_approval_requests_driver_id",
                table: "driver_approval_requests");

            migrationBuilder.AlterColumn<decimal>(
                name: "pick_up_lng",
                table: "students",
                type: "decimal(10,6)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double");

            migrationBuilder.AlterColumn<decimal>(
                name: "pick_up_lat",
                table: "students",
                type: "decimal(10,6)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double");
        }
    }
}
