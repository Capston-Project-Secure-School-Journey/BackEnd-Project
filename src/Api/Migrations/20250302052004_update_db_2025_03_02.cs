using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class update_db_2025_03_02 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "file_managements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    file_name = table.Column<string>(type: "nvarchar(200)", nullable: false),
                    s3_key = table.Column<string>(type: "varchar(2000)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    file_type = table.Column<string>(type: "varchar(100)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    file_size = table.Column<float>(type: "float", nullable: false),
                    upload_date = table.Column<DateTime>(type: "timestamp", nullable: false),
                    uploaded_by = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    related_object_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    related_object_type = table.Column<sbyte>(type: "tinyint", nullable: true),
                    is_uploaded = table.Column<ulong>(type: "bit", nullable: false, defaultValue: 0ul),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_file_managements", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "file_managements");
        }
    }
}
