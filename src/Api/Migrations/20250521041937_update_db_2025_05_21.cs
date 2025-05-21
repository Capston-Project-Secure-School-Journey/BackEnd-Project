using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class update_db_2025_05_21 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActiveDrivers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    driver_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    school_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    seating_capacity = table.Column<sbyte>(type: "tinyint", nullable: false),
                    used = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ExpiredAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActiveDrivers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActiveDrivers_schools_school_id",
                        column: x => x.school_id,
                        principalTable: "schools",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_ActiveDrivers_users_driver_id",
                        column: x => x.driver_id,
                        principalTable: "users",
                        principalColumn: "id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ActiveDrivers_driver_id_school_id",
                table: "ActiveDrivers",
                columns: new[] { "driver_id", "school_id" });

            migrationBuilder.CreateIndex(
                name: "IX_ActiveDrivers_school_id",
                table: "ActiveDrivers",
                column: "school_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActiveDrivers");
        }
    }
}
