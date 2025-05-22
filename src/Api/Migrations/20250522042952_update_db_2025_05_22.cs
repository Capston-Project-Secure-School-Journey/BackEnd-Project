using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class update_db_2025_05_22 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActiveDrivers_schools_school_id",
                table: "ActiveDrivers");

            migrationBuilder.DropForeignKey(
                name: "FK_ActiveDrivers_users_driver_id",
                table: "ActiveDrivers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ActiveDrivers",
                table: "ActiveDrivers");

            migrationBuilder.RenameTable(
                name: "ActiveDrivers",
                newName: "active_drivers");

            migrationBuilder.RenameIndex(
                name: "IX_ActiveDrivers_school_id",
                table: "active_drivers",
                newName: "IX_active_drivers_school_id");

            migrationBuilder.RenameIndex(
                name: "IX_ActiveDrivers_driver_id_school_id",
                table: "active_drivers",
                newName: "IX_active_drivers_driver_id_school_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_active_drivers",
                table: "active_drivers",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_active_drivers_schools_school_id",
                table: "active_drivers",
                column: "school_id",
                principalTable: "schools",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_active_drivers_users_driver_id",
                table: "active_drivers",
                column: "driver_id",
                principalTable: "users",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_active_drivers_schools_school_id",
                table: "active_drivers");

            migrationBuilder.DropForeignKey(
                name: "FK_active_drivers_users_driver_id",
                table: "active_drivers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_active_drivers",
                table: "active_drivers");

            migrationBuilder.RenameTable(
                name: "active_drivers",
                newName: "ActiveDrivers");

            migrationBuilder.RenameIndex(
                name: "IX_active_drivers_school_id",
                table: "ActiveDrivers",
                newName: "IX_ActiveDrivers_school_id");

            migrationBuilder.RenameIndex(
                name: "IX_active_drivers_driver_id_school_id",
                table: "ActiveDrivers",
                newName: "IX_ActiveDrivers_driver_id_school_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ActiveDrivers",
                table: "ActiveDrivers",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ActiveDrivers_schools_school_id",
                table: "ActiveDrivers",
                column: "school_id",
                principalTable: "schools",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_ActiveDrivers_users_driver_id",
                table: "ActiveDrivers",
                column: "driver_id",
                principalTable: "users",
                principalColumn: "id");
        }
    }
}
