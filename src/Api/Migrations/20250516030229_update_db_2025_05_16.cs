using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class update_db_2025_05_16 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_driver_approval_requests_users_driver_id",
                table: "driver_approval_requests");

            migrationBuilder.DropForeignKey(
                name: "FK_driver_request_status_histories_driver_approval_requests_req~",
                table: "driver_request_status_histories");

            migrationBuilder.AlterColumn<string>(
                name: "note",
                table: "driver_request_status_histories",
                type: "nvarchar(1000)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)");

            migrationBuilder.CreateIndex(
                name: "IX_driver_approval_requests_school_id",
                table: "driver_approval_requests",
                column: "school_id");

            migrationBuilder.AddForeignKey(
                name: "FK_driver_approval_requests_schools_school_id",
                table: "driver_approval_requests",
                column: "school_id",
                principalTable: "schools",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_driver_approval_requests_users_driver_id",
                table: "driver_approval_requests",
                column: "driver_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_driver_request_status_histories_driver_approval_requests_req~",
                table: "driver_request_status_histories",
                column: "request_id",
                principalTable: "driver_approval_requests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_driver_approval_requests_schools_school_id",
                table: "driver_approval_requests");

            migrationBuilder.DropForeignKey(
                name: "FK_driver_approval_requests_users_driver_id",
                table: "driver_approval_requests");

            migrationBuilder.DropForeignKey(
                name: "FK_driver_request_status_histories_driver_approval_requests_req~",
                table: "driver_request_status_histories");

            migrationBuilder.DropIndex(
                name: "IX_driver_approval_requests_school_id",
                table: "driver_approval_requests");

            migrationBuilder.UpdateData(
                table: "driver_request_status_histories",
                keyColumn: "note",
                keyValue: null,
                column: "note",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "note",
                table: "driver_request_status_histories",
                type: "nvarchar(1000)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_driver_approval_requests_users_driver_id",
                table: "driver_approval_requests",
                column: "driver_id",
                principalTable: "users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_driver_request_status_histories_driver_approval_requests_req~",
                table: "driver_request_status_histories",
                column: "request_id",
                principalTable: "driver_approval_requests",
                principalColumn: "Id");
        }
    }
}
