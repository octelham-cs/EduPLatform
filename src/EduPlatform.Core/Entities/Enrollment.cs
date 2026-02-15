namespace EduPlatform.Core.Entities
{
    // اشتراك الطالب في كورس
    public class Enrollment
    {
        public int Id { get; set; }

        // الطالب
        public int StudentId { get; set; }

        // الكورس
        public int CourseId { get; set; }

        // كود الاشتراك
        public int? EnrollmentCodeId { get; set; }

        // حالة الاشتراك (نشط، منتهي)
        public bool IsActive { get; set; } = true;

        // تاريخ الاشتراك
        public DateTime EnrolledAt { get; set; } = DateTime.Now;

        // تاريخ انتهاء الاشتراك
        public DateTime ExpiresAt { get; set; }

        // Navigation Properties
        public Student Student { get; set; } = null!;
        public Course Course { get; set; } = null!;
    }
}