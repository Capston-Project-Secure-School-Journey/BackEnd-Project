using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class update_db_2025_03_04_01 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "full_name",
                table: "teachers",
                type: "nvarchar(400)",
                nullable: false,
                computedColumnSql: "CONCAT(first_name, ' ', last_name)",
                oldClrType: typeof(string),
                oldType: "longtext",
                oldComputedColumnSql: "CONCAT(first_name, ' ', last_name)")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "full_name",
                table: "students",
                type: "nvarchar(400)",
                nullable: false,
                computedColumnSql: "CONCAT(first_name, ' ', last_name)",
                oldClrType: typeof(string),
                oldType: "longtext",
                oldComputedColumnSql: "CONCAT(first_name, ' ', last_name)")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "full_name",
                table: "teachers",
                type: "longtext",
                nullable: false,
                computedColumnSql: "CONCAT(first_name, ' ', last_name)",
                oldClrType: typeof(string),
                oldType: "nvarchar(400)",
                oldComputedColumnSql: "CONCAT(first_name, ' ', last_name)")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "full_name",
                table: "students",
                type: "longtext",
                nullable: false,
                computedColumnSql: "CONCAT(first_name, ' ', last_name)",
                oldClrType: typeof(string),
                oldType: "nvarchar(400)",
                oldComputedColumnSql: "CONCAT(first_name, ' ', last_name)")
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
