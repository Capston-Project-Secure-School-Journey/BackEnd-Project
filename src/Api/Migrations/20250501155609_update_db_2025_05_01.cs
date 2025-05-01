using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class update_db_2025_05_01 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS trg_users_bi_usertype;");
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS trg_sync_student;");
            
            migrationBuilder.Sql(@"
                CREATE TRIGGER trg_users_bi_usertype
                BEFORE INSERT ON users
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
                END;");
            
            migrationBuilder.Sql(@"
                CREATE DEFINER=`admin`@`%` TRIGGER `trg_sync_student` BEFORE UPDATE ON `users` FOR EACH ROW BEGIN
                    DECLARE i INT DEFAULT 0;
                    DECLARE j INT DEFAULT 0;
                    DECLARE student_id_new VARCHAR(36);
                    DECLARE student_id_old VARCHAR(36);
                    DECLARE rel_value INT;
                    DECLARE path_index VARCHAR(10);
                    DECLARE found_position TEXT;
                    DECLARE parent_index INT;
                
                    -- Process new relationships
                    WHILE i < JSON_LENGTH(IFNULL(NEW.relationship_with_students, JSON_ARRAY())) DO
                        SET student_id_new = JSON_UNQUOTE(JSON_EXTRACT(NEW.relationship_with_students, CONCAT('$[', i, '].StudentId')));
                        SET rel_value = CAST(JSON_EXTRACT(NEW.relationship_with_students, CONCAT('$[', i, '].Relationship')) AS UNSIGNED);
                
                        -- Check if this relationship is new
                        IF (JSON_SEARCH(IFNULL(OLD.relationship_with_students, JSON_ARRAY()), 'one', student_id_new) IS NULL) THEN
                            UPDATE students
                            SET managed_by = JSON_ARRAY_APPEND(
                                IFNULL(managed_by, JSON_ARRAY()),
                                '$',
                                JSON_OBJECT(
                                    'ParentId', NEW.id,
                                    'RelationshipWithStudent', rel_value
                                )
                            )
                            WHERE id = student_id_new;
                        END IF;
                
                        SET i = i + 1;
                    END WHILE;
                
                    -- Process removed relationships
                    SET i = 0;
                    WHILE i < JSON_LENGTH(IFNULL(OLD.relationship_with_students, JSON_ARRAY())) DO
                        SET student_id_old = JSON_UNQUOTE(JSON_EXTRACT(OLD.relationship_with_students, CONCAT('$[', i, '].StudentId')));
                
                        -- Check if this relationship has been removed
                        IF (JSON_SEARCH(IFNULL(NEW.relationship_with_students, JSON_ARRAY()), 'one', student_id_old) IS NULL) THEN
                            -- Find the parent in the managed_by array manually since the path is causing problems
                            SET parent_index = -1;
                            SET j = 0;
                            
                            -- Get the managed_by data for this student
                            SELECT s.managed_by INTO @managed_data
                            FROM students s
                            WHERE s.id = student_id_old;
                            
                            -- Find the index where ParentId matches OLD.id
                            WHILE j < JSON_LENGTH(IFNULL(@managed_data, JSON_ARRAY())) DO
                                IF JSON_UNQUOTE(JSON_EXTRACT(@managed_data, CONCAT('$[', j, '].ParentId'))) = OLD.id THEN
                                    SET parent_index = j;
                                END IF;
                                SET j = j + 1;
                            END WHILE;
                            
                            -- Remove the relationship if found
                            IF parent_index >= 0 THEN
                                UPDATE students
                                SET managed_by = JSON_REMOVE(managed_by, CONCAT('$[', parent_index, ']'))
                                WHERE id = student_id_old;
                            END IF;
                        END IF;
                
                        SET i = i + 1;
                    END WHILE;
                END;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS trg_users_bi_usertype;");
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS trg_sync_student;");
        }
    }
}
