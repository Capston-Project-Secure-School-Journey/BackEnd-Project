using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class update_db_2025_03_02_2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "full_name",
                table: "teachers",
                type: "longtext",
                nullable: false,
                computedColumnSql: "CONCAT(first_name, ' ', last_name)")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "full_name",
                table: "students",
                type: "longtext",
                nullable: false,
                computedColumnSql: "CONCAT(first_name, ' ', last_name)")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "full_name",
                table: "teachers");

            migrationBuilder.DropColumn(
                name: "full_name",
                table: "students");
        }
    }
}
