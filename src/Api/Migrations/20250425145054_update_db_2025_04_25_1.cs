using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class update_db_2025_04_25_1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notification_users_recipient_id",
                table: "Notification");

            migrationBuilder.DropForeignKey(
                name: "FK_Notification_users_sender_id",
                table: "Notification");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Notification",
                table: "Notification");

            migrationBuilder.DropIndex(
                name: "IX_Notification_recipient_id",
                table: "Notification");

            migrationBuilder.DropIndex(
                name: "IX_Notification_sender_id",
                table: "Notification");

            migrationBuilder.DropColumn(
                name: "id",
                table: "Notification");

            migrationBuilder.DropColumn(
                name: "createdAt",
                table: "Notification");

            migrationBuilder.DropColumn(
                name: "is_read",
                table: "Notification");

            migrationBuilder.DropColumn(
                name: "navigation",
                table: "Notification");

            migrationBuilder.DropColumn(
                name: "priority",
                table: "Notification");

            migrationBuilder.DropColumn(
                name: "sender_id",
                table: "Notification");

            migrationBuilder.DropColumn(
                name: "title",
                table: "Notification");

            migrationBuilder.DropColumn(
                name: "type",
                table: "Notification");

            migrationBuilder.RenameColumn(
                name: "recipient_id",
                table: "Notification",
                newName: "school_id");

            migrationBuilder.RenameColumn(
                name: "content",
                table: "Notification",
                newName: "value");

            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "Notification",
                type: "varchar(100)",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Notification",
                table: "Notification",
                columns: new[] { "school_id", "name" });

            migrationBuilder.CreateTable(
                name: "driver_approval_requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    school_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    requested_date = table.Column<DateTime>(type: "datetime", nullable: false),
                    MotivationLetter = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    driver_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    request_status = table.Column<sbyte>(type: "tinyint", nullable: false),
                    approved_by = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    vehicle_type = table.Column<string>(type: "nvarchar(200)", nullable: false),
                    license_number = table.Column<string>(type: "varchar(50)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SeatingCapacity = table.Column<int>(type: "int", nullable: false),
                    last_check_driving_license = table.Column<DateTime>(type: "datetime", nullable: false),
                    driver_information_images = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    vehicle_images = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_driver_approval_requests", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    title = table.Column<string>(type: "nvarchar(200)", nullable: false),
                    content = table.Column<string>(type: "nvarchar(1000)", nullable: false),
                    type = table.Column<sbyte>(type: "tinyint", nullable: false),
                    recipient_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    sender_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    createdAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    is_read = table.Column<ulong>(type: "bit", nullable: false),
                    navigation = table.Column<string>(type: "varchar(300)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    priority = table.Column<sbyte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.id);
                    table.ForeignKey(
                        name: "FK_notifications_users_recipient_id",
                        column: x => x.recipient_id,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_notifications_users_sender_id",
                        column: x => x.sender_id,
                        principalTable: "users",
                        principalColumn: "id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "driver_request_status_histories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    request_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    from_status = table.Column<sbyte>(type: "tinyint", nullable: true),
                    to_status = table.Column<sbyte>(type: "tinyint", nullable: false),
                    changed_by = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    changed_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    note = table.Column<string>(type: "nvarchar(1000)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_driver_request_status_histories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_driver_request_status_histories_driver_approval_requests_req~",
                        column: x => x.request_id,
                        principalTable: "driver_approval_requests",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_driver_request_status_histories_request_id",
                table: "driver_request_status_histories",
                column: "request_id");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_recipient_id",
                table: "notifications",
                column: "recipient_id");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_sender_id",
                table: "notifications",
                column: "sender_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "driver_request_status_histories");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "driver_approval_requests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Notification",
                table: "Notification");

            migrationBuilder.DropColumn(
                name: "name",
                table: "Notification");

            migrationBuilder.RenameColumn(
                name: "value",
                table: "Notification",
                newName: "content");

            migrationBuilder.RenameColumn(
                name: "school_id",
                table: "Notification",
                newName: "recipient_id");

            migrationBuilder.AddColumn<Guid>(
                name: "id",
                table: "Notification",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<DateTime>(
                name: "createdAt",
                table: "Notification",
                type: "datetime",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<ulong>(
                name: "is_read",
                table: "Notification",
                type: "bit",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddColumn<string>(
                name: "navigation",
                table: "Notification",
                type: "varchar(300)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<sbyte>(
                name: "priority",
                table: "Notification",
                type: "tinyint",
                nullable: false,
                defaultValue: (sbyte)0);

            migrationBuilder.AddColumn<Guid>(
                name: "sender_id",
                table: "Notification",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<string>(
                name: "title",
                table: "Notification",
                type: "nvarchar(200)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<sbyte>(
                name: "type",
                table: "Notification",
                type: "tinyint",
                nullable: false,
                defaultValue: (sbyte)0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Notification",
                table: "Notification",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_recipient_id",
                table: "Notification",
                column: "recipient_id");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_sender_id",
                table: "Notification",
                column: "sender_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Notification_users_recipient_id",
                table: "Notification",
                column: "recipient_id",
                principalTable: "users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Notification_users_sender_id",
                table: "Notification",
                column: "sender_id",
                principalTable: "users",
                principalColumn: "id");
        }
    }
}
