using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EduPlatform.Core.Enums;

namespace EduPlatform.Core.Entities
{
    public class EnrollmentCode
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty;

        public int InstructorId { get; set; }
        public int CourseId { get; set; }
        public int AcademicTermId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public CodeStatus Status { get; set; } = CodeStatus.Available;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? UsedAt { get; set; }

        // ✅ Foreign Key للطالب (اللي استخدم الكود)
        public int? UsedBy { get; set; }

        // ✅ Navigation Properties
        [ForeignKey(nameof(InstructorId))]
        public Instructor Instructor { get; set; } = null!;

        [ForeignKey(nameof(CourseId))]
        public Course Course { get; set; } = null!;

        [ForeignKey(nameof(AcademicTermId))]
        public AcademicTerm AcademicTerm { get; set; } = null!;

        [ForeignKey(nameof(UsedBy))]
        public Student? Student { get; set; }
    }
}