using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class update_db_2025_02_22 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TeacherId",
                table: "classes",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_classes_TeacherId",
                table: "classes",
                column: "TeacherId");

            migrationBuilder.AddForeignKey(
                name: "FK_classes_teachers_TeacherId",
                table: "classes",
                column: "TeacherId",
                principalTable: "teachers",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_classes_teachers_TeacherId",
                table: "classes");

            migrationBuilder.DropIndex(
                name: "IX_classes_TeacherId",
                table: "classes");

            migrationBuilder.DropColumn(
                name: "TeacherId",
                table: "classes");
        }
    }
}
