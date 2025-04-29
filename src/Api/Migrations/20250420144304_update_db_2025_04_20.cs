using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class update_db_2025_04_20 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "device_tokens",
                table: "users",
                type: "json",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
            
            migrationBuilder.Sql("UPDATE users SET device_tokens = '[]'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "device_tokens",
                table: "users");
        }
    }
}
