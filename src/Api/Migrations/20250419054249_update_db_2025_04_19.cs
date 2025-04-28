using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class update_db_2025_04_19 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<sbyte>(
                name: "needs_pickup",
                table: "students",
                type: "tinyint",
                nullable: false,
                defaultValue: (sbyte)0);
            
            migrationBuilder.Sql("UPDATE students SET needs_pickup = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "needs_pickup",
                table: "students");
        }
    }
}
