using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class update_db_2025_04_30 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Notification",
                table: "Notification");

            migrationBuilder.RenameTable(
                name: "Notification",
                newName: "system_variables");

            migrationBuilder.AddPrimaryKey(
                name: "PK_system_variables",
                table: "system_variables",
                columns: new[] { "school_id", "name" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_system_variables",
                table: "system_variables");

            migrationBuilder.RenameTable(
                name: "system_variables",
                newName: "Notification");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Notification",
                table: "Notification",
                columns: new[] { "school_id", "name" });
        }
    }
}
