using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class update_db_2025_03_04 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "relationship_with_student",
                table: "users");

            migrationBuilder.AddColumn<string>(
                name: "relationship_with_students",
                table: "users",
                type: "json",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "relationship_with_students",
                table: "users");

            migrationBuilder.AddColumn<sbyte>(
                name: "relationship_with_student",
                table: "users",
                type: "tinyint",
                nullable: true);
        }
    }
}
