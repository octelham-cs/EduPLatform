using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEnrollmentCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EnrollmentCodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InstructorId = table.Column<int>(type: "int", nullable: false),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    AcademicTermId = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountPercentage = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsedByStudentId = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnrollmentCodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EnrollmentCodes_AcademicTerms_AcademicTermId",
                        column: x => x.AcademicTermId,
                        principalTable: "AcademicTerms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EnrollmentCodes_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EnrollmentCodes_Instructors_InstructorId",
                        column: x => x.InstructorId,
                        principalTable: "Instructors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_EnrollmentCodes_Students_UsedByStudentId",
                        column: x => x.UsedByStudentId,
                        principalTable: "Students",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_EnrollmentCodes_AcademicTermId",
                table: "EnrollmentCodes",
                column: "AcademicTermId");

            migrationBuilder.CreateIndex(
                name: "IX_EnrollmentCodes_CourseId",
                table: "EnrollmentCodes",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_EnrollmentCodes_InstructorId",
                table: "EnrollmentCodes",
                column: "InstructorId");

            migrationBuilder.CreateIndex(
                name: "IX_EnrollmentCodes_UsedByStudentId",
                table: "EnrollmentCodes",
                column: "UsedByStudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EnrollmentCodes");
        }
    }
}
