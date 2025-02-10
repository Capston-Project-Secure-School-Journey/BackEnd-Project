using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class update_school_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_users_SchoolId",
                table: "users",
                column: "SchoolId");

            migrationBuilder.AddForeignKey(
                name: "FK_users_schools_SchoolId",
                table: "users",
                column: "SchoolId",
                principalTable: "schools",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_users_schools_SchoolId",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_users_SchoolId",
                table: "users");
        }
    }
}
