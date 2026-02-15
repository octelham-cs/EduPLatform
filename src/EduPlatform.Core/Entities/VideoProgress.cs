namespace EduPlatform.Core.Entities
{
    // تتبع مشاهدة الفيديو للطالب
    public class VideoProgress
    {
        public int Id { get; set; }

        // الفيديو
        public int VideoId { get; set; }

        // الطالب
        public int StudentId { get; set; }

        // هل اكتملت المشاهدة؟
        public bool IsCompleted { get; set; } = false;

        // آخر موقف وصل له الطالب (بالثواني)
        public int LastPosition { get; set; } = 0;

        // آخر مرة شاهد فيها الفيديو
        public DateTime LastWatchedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        public Video Video { get; set; } = null!;
        public Student Student { get; set; } = null!;
    }
}