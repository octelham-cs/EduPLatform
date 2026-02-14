namespace EduPlatform.Core.Entities
{
    // المادة الدراسية (عربي، رياضيات، فيزياء، إلخ)
    public class Subject
    {
        public int Id { get; set; }

        // اسم المادة بالعربية
        public string Name { get; set; } = string.Empty;

        // اسم المادة بالإنجليزية
        public string NameEn { get; set; } = string.Empty;

        // وصف المادة
        public string? Description { get; set; }

        // المستوى الدراسي
        public int GradeLevelId { get; set; }

        // الشعبة (null = عامة لكل الشعب)
        public int? BranchId { get; set; }

        // هل المادة نشطة؟
        public bool IsActive { get; set; } = true;
    }
}