using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSubjecttoInstructor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // إضافة العمود SubjectId إذا لم يكن موجوداً
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Instructors' AND COLUMN_NAME = 'SubjectId')
                BEGIN
                    ALTER TABLE Instructors ADD SubjectId INT NULL;
                END
            ");

            // تحديث البيانات (افتراضياً خلي أول مادة)
            migrationBuilder.Sql(@"
                UPDATE Instructors SET SubjectId = 1 WHERE SubjectId IS NULL;
            ");

            // جعل العمود Required
            migrationBuilder.Sql(@"
                ALTER TABLE Instructors ALTER COLUMN SubjectId INT NOT NULL;
            ");

            // إنشاء Index إذا لم يكن موجوداً
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Instructors_SubjectId' AND object_id = OBJECT_ID('Instructors'))
                BEGIN
                    CREATE INDEX [IX_Instructors_SubjectId] ON [Instructors] ([SubjectId]);
                END
            ");

            // إنشاء Foreign Key إذا لم تكن موجودة
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Instructors_Subjects_SubjectId')
                BEGIN
                    ALTER TABLE [Instructors] 
                    ADD CONSTRAINT [FK_Instructors_Subjects_SubjectId] 
                    FOREIGN KEY ([SubjectId]) REFERENCES [Subjects]([Id]) 
                    ON DELETE NO ACTION;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // حذف Foreign Key إذا كانت موجودة
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Instructors_Subjects_SubjectId')
                BEGIN
                    ALTER TABLE [Instructors] DROP CONSTRAINT [FK_Instructors_Subjects_SubjectId];
                END
            ");

            // حذف Index إذا كان موجوداً
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Instructors_SubjectId' AND object_id = OBJECT_ID('Instructors'))
                BEGIN
                    DROP INDEX [IX_Instructors_SubjectId] ON [Instructors];
                END
            ");

            // حذف العمود SubjectId إذا كان موجوداً
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Instructors' AND COLUMN_NAME = 'SubjectId')
                BEGIN
                    ALTER TABLE Instructors DROP COLUMN SubjectId;
                END
            ");
        }
    }
}