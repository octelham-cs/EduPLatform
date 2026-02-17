namespace EduPlatform.Web.ViewModels.Student
{
    public class StudentDashboardViewModel
    {
        public List<EnrolledCourseViewModel> EnrolledCourses { get; set; }
        public int TotalEnrollments { get; set; }
        public int ActiveEnrollments { get; set; }
        public int CompletedCourses { get; set; }
    }

    public class EnrolledCourseViewModel
    {
        public int EnrollmentId { get; set; }
        public int CourseId { get; set; }
        public string CourseTitle { get; set; }
        public string CourseDescription { get; set; }
        public string InstructorName { get; set; }
        public string InstructorProfilePicture { get; set; }
        public string ThumbnailUrl { get; set; }
        public decimal Price { get; set; }
        public DateTime EnrolledAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public int TotalVideos { get; set; }
        public int WatchedVideos { get; set; }
        public int ProgressPercentage { get; set; }
        public bool IsActive { get; set; }
        public bool IsExpiringSoon { get; set; }
    }
}