using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class update_db_2025_03_27 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "schedule_group_id",
                table: "class_schedules",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateTable(
                name: "schedule_groups",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    school_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    session_type = table.Column<sbyte>(type: "tinyint", nullable: false),
                    schedule_type = table.Column<sbyte>(type: "tinyint", nullable: false),
                    grade = table.Column<sbyte>(type: "tinyint", nullable: true),
                    ClassException = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_schedule_groups", x => x.id);
                    table.ForeignKey(
                        name: "FK_schedule_groups_schools_school_id",
                        column: x => x.school_id,
                        principalTable: "schools",
                        principalColumn: "id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_class_schedules_schedule_group_id",
                table: "class_schedules",
                column: "schedule_group_id");

            migrationBuilder.CreateIndex(
                name: "IX_schedule_groups_school_id",
                table: "schedule_groups",
                column: "school_id");

            migrationBuilder.AddForeignKey(
                name: "FK_class_schedules_schedule_groups_schedule_group_id",
                table: "class_schedules",
                column: "schedule_group_id",
                principalTable: "schedule_groups",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_class_schedules_schedule_groups_schedule_group_id",
                table: "class_schedules");

            migrationBuilder.DropTable(
                name: "schedule_groups");

            migrationBuilder.DropIndex(
                name: "IX_class_schedules_schedule_group_id",
                table: "class_schedules");

            migrationBuilder.DropColumn(
                name: "schedule_group_id",
                table: "class_schedules");
        }
    }
}
