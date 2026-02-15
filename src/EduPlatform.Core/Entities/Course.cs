using System.Collections.Generic;

namespace EduPlatform.Core.Entities
{
    // الكورس = مادة يدرسها مدرس معين
    public class Course
    {
        public int Id { get; set; }

        // المدرس
        public int InstructorId { get; set; }

        // المادة الدراسية
        public int SubjectId { get; set; }

        // عنوان الكورس
        public string Title { get; set; } = string.Empty;

        // وصف الكورس
        public string? Description { get; set; }

        // سعر الكورس
        public decimal Price { get; set; }

        // هل الكورس نشط؟
        public bool IsActive { get; set; } = true;

        // تاريخ الإنشاء
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        public Instructor Instructor { get; set; } = null!;
        public Subject Subject { get; set; } = null!;

        // الفصول داخل الكورس
        public ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();
    }
}