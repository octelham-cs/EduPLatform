using System.Collections.Generic;

namespace EduPlatform.Core.Entities
{
    public class Video
    {
        public int Id { get; set; }

        // الفصل/القسم
        public int ChapterId { get; set; }

        // عنوان الفيديو
        public string Title { get; set; } = string.Empty;

        // وصف الفيديو
        public string? Description { get; set; }

        // رابط يوتيوب
        public string YouTubeUrl { get; set; } = string.Empty;

        // معرف الفيديو من يوتيوب (للاستخراج)
        public string? YouTubeVideoId { get; set; }

        // صورة مصغرة (من يوتيوب)
        public string? ThumbnailUrl { get; set; }

        // مدة الفيديو بالثواني
        public int DurationSeconds { get; set; }

        // ترتيب الفيديو داخل الفصل
        public int Order { get; set; } = 0;

        // هل الفيديو مخفي؟
        public bool IsHidden { get; set; } = false;

        // تاريخ الإنشاء
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        public Chapter Chapter { get; set; } = null!;

        // الملفات المرفقة
        public ICollection<AttachedFile> AttachedFiles { get; set; } = new List<AttachedFile>();
    }
}