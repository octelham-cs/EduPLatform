namespace EduPlatform.Core.Entities
{
    // الفصل/القسم داخل الكورس
    public class Chapter
    {
        public int Id { get; set; }

        // الكورس
        public int CourseId { get; set; }

        // عنوان الفصل
        public string Title { get; set; } = string.Empty;

        // وصف الفصل
        public string? Description { get; set; }

        // ترتيب العرض
        public int Order { get; set; } = 0;

        // هل الفصل نشط؟
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public Course Course { get; set; } = null!;
    }
}