using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class update_db_2025_03_04_02 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE TRIGGER SetUserTypeName BEFORE INSERT ON users
                        FOR EACH ROW
                        BEGIN
                            SET NEW.user_type_name = 
                                CASE 
                                    WHEN NEW.user_type = 1 THEN 'SchoolAdmin'
                                    WHEN NEW.user_type = 2 THEN 'SchoolSuperVisor'
                                    WHEN NEW.user_type = 3 THEN 'Driver'
                                    WHEN NEW.user_type = 4 THEN 'Parent'
                                    WHEN NEW.user_type = 5 THEN 'Admin'
                                    ELSE 'Unknown'
                                END;
                        END;
                ");
            
            migrationBuilder.RenameColumn(
                name: "user_type1",
                table: "users",
                newName: "discriminator");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "date_of_birth",
                table: "students",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP TRIGGER IF EXISTS SetUserTypeName;
                ");
            migrationBuilder.RenameColumn(
                name: "discriminator",
                table: "users",
                newName: "user_type1");

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_of_birth",
                table: "students",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");
        }
    }
}
