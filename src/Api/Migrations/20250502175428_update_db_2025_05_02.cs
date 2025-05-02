using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class update_db_2025_05_02 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "avatar_url",
                table: "users",
                newName: "avatar_key");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "system_variables",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "system_variables",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "system_variables",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "schedule_groups",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "schedule_groups",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "schedule_groups",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "system_variables");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "system_variables");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "system_variables");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "schedule_groups");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "schedule_groups");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "schedule_groups");

            migrationBuilder.RenameColumn(
                name: "avatar_key",
                table: "users",
                newName: "avatar_url");
        }
    }
}
