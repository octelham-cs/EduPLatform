namespace EduPlatform.Web.ViewModels.Student
{
    public class VideoPlayerViewModel
    {
        public int VideoId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string YouTubeUrl { get; set; }
        public string YouTubeEmbedUrl { get; set; }
        public int DurationSeconds { get; set; }

        public int CourseId { get; set; }
        public string CourseTitle { get; set; }

        public int ChapterId { get; set; }
        public string ChapterTitle { get; set; }

        // الفيديو السابق والتالي
        public int? PreviousVideoId { get; set; }
        public int? NextVideoId { get; set; }

        // الملفات المرفقة
        public List<AttachedFileViewModel> AttachedFiles { get; set; }

        // اختبار الفيديو (لو موجود)
        public bool HasQuiz { get; set; }
        public int? QuizId { get; set; }
        public bool QuizPassed { get; set; }

        // تقدم المشاهدة
        public int LastPosition { get; set; }
        public bool IsCompleted { get; set; }
    }

    public class AttachedFileViewModel
    {
        public int FileId { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string FileType { get; set; }
        public long FileSize { get; set; }
        public string FormattedFileSize { get; set; }
    }
}