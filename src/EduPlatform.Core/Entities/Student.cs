namespace EduPlatform.Core.Entities
{
    public class Student
    {
        public int Id { get; set; }

        // ربط بـ ApplicationUser
        public string UserId { get; set; } = string.Empty;

        // المستوى الدراسي
        public int GradeLevelId { get; set; }

        // الشعبة (اختياري)
        public int? BranchId { get; set; }

        // تاريخ التسجيل
        public DateTime RegisteredAt { get; set; } = DateTime.Now;

        // Navigation Properties
        public ApplicationUser User { get; set; } = null!;
        public GradeLevel GradeLevel { get; set; } = null!;
        public Branch? Branch { get; set; }
    }
}