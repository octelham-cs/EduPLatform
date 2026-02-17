namespace EduPlatform.Web.ViewModels.Student
{
    public class CourseDetailsViewModel
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; }
        public string CourseDescription { get; set; }
        public string InstructorName { get; set; }
        public string InstructorBio { get; set; }
        public string InstructorProfilePicture { get; set; }
        public DateTime EnrolledAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public int TotalVideos { get; set; }
        public int WatchedVideos { get; set; }
        public int ProgressPercentage { get; set; }

        public List<ChapterViewModel> Chapters { get; set; }
    }

    public class ChapterViewModel
    {
        public int ChapterId { get; set; }
        public string Title { get; set; }
        public int Order { get; set; }
        public List<VideoItemViewModel> Videos { get; set; }
    }

    public class VideoItemViewModel
    {
        public int VideoId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ThumbnailUrl { get; set; }
        public int DurationSeconds { get; set; }
        public int Order { get; set; }
        public bool IsWatched { get; set; }
        public bool IsLocked { get; set; }
        public bool HasQuiz { get; set; }
        public int? QuizId { get; set; }
        public int? LastPosition { get; set; }
        public string YouTubeUrl { get; set; }
    }
}