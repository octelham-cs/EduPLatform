namespace EduPlatform.Core.Entities
{
    // الملفات المرفقة مع الفيديو (PDF, Word, Excel)
    public class AttachedFile
    {
        public int Id { get; set; }

        // الفيديو المرتبط
        public int VideoId { get; set; }

        // اسم الملف الأصلي
        public string FileName { get; set; } = string.Empty;

        // مسار الملف على السيرفر
        public string FilePath { get; set; } = string.Empty;

        // نوع الملف
        public string FileType { get; set; } = string.Empty;

        // حجم الملف بالبايت
        public long FileSize { get; set; }

        // تاريخ الرفع
        public DateTime UploadedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        public Video Video { get; set; } = null!;
    }
}