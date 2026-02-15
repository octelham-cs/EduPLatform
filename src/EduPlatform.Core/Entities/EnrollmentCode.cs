using EduPlatform.Core.Enums;

namespace EduPlatform.Core.Entities
{
    // كود الاشتراك
    public class EnrollmentCode
    {
        public int Id { get; set; }

        // الكود (مثال: EDU-AR1-3A9F)
        public string Code { get; set; } = string.Empty;

        // المدرس
        public int InstructorId { get; set; }

        // الكورس
        public int CourseId { get; set; }

        // الترم الدراسي
        public int AcademicTermId { get; set; }

        // السعر
        public decimal Price { get; set; }

        // نسبة الخصم
        public int? DiscountPercentage { get; set; }

        // حالة الكود
        public CodeStatus Status { get; set; } = CodeStatus.Available;

        // تاريخ الإنشاء
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // تاريخ البداية
        public DateTime StartDate { get; set; }

        // تاريخ النهاية
        public DateTime EndDate { get; set; }

        // تاريخ الاستخدام
        public DateTime? UsedAt { get; set; }

        // الطالب المستخدم (لو مستخدم)
        public int? UsedByStudentId { get; set; }

        // ملاحظات
        public string? Notes { get; set; }

        // Navigation Properties
        public Instructor Instructor { get; set; } = null!;
        public Course Course { get; set; } = null!;
        public AcademicTerm AcademicTerm { get; set; } = null!;
        public Student? UsedByStudent { get; set; }
    }
}