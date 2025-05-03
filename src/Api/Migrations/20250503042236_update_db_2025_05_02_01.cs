using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class update_db_2025_05_02_01 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_notifications_users_recipient_id",
                table: "notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_notifications_users_sender_id",
                table: "notifications");

            migrationBuilder.AddForeignKey(
                name: "FK_notifications_users_recipient_id",
                table: "notifications",
                column: "recipient_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_notifications_users_sender_id",
                table: "notifications",
                column: "sender_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_notifications_users_recipient_id",
                table: "notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_notifications_users_sender_id",
                table: "notifications");

            migrationBuilder.AddForeignKey(
                name: "FK_notifications_users_recipient_id",
                table: "notifications",
                column: "recipient_id",
                principalTable: "users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_notifications_users_sender_id",
                table: "notifications",
                column: "sender_id",
                principalTable: "users",
                principalColumn: "id");
        }
    }
}
