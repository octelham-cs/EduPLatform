using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixUsedByColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EnrollmentCodes_Students_UsedByStudentId",
                table: "EnrollmentCodes");

            migrationBuilder.DropColumn(
                name: "DiscountPercentage",
                table: "EnrollmentCodes");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "EnrollmentCodes");

            migrationBuilder.RenameColumn(
                name: "UsedByStudentId",
                table: "EnrollmentCodes",
                newName: "UsedBy");

            migrationBuilder.RenameIndex(
                name: "IX_EnrollmentCodes_UsedByStudentId",
                table: "EnrollmentCodes",
                newName: "IX_EnrollmentCodes_UsedBy");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "EnrollmentCodes",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            // ✅ تغيير هذا السطر - إزالة onDelete
            migrationBuilder.AddForeignKey(
                name: "FK_EnrollmentCodes_Students_UsedBy",
                table: "EnrollmentCodes",
                column: "UsedBy",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict); // تغيير من SetNull إلى Restrict

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Chapters_ChapterId",
                table: "Questions",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EnrollmentCodes_Students_UsedBy",
                table: "EnrollmentCodes");

            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Chapters_ChapterId",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_Questions_ChapterId",
                table: "Questions");

            migrationBuilder.RenameColumn(
                name: "UsedBy",
                table: "EnrollmentCodes",
                newName: "UsedByStudentId");

            migrationBuilder.RenameIndex(
                name: "IX_EnrollmentCodes_UsedBy",
                table: "EnrollmentCodes",
                newName: "IX_EnrollmentCodes_UsedByStudentId");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "EnrollmentCodes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<int>(
                name: "DiscountPercentage",
                table: "EnrollmentCodes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "EnrollmentCodes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EnrollmentCodes_Students_UsedByStudentId",
                table: "EnrollmentCodes",
                column: "UsedByStudentId",
                principalTable: "Students",
                principalColumn: "Id");
        }
    }
}