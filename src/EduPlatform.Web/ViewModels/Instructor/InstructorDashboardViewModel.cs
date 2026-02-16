// src/EduPlatform.Web/ViewModels/Instructor/InstructorDashboardViewModel.cs

using System.Collections.Generic;

namespace EduPlatform.Web.ViewModels.Instructor
{
    public class InstructorDashboardViewModel
    {
        public int TotalCourses { get; set; }
        public int TotalVideos { get; set; }
        public int TotalStudents { get; set; }
        public int ActiveCodes { get; set; }
        public int AvailableCodes { get; set; }
        public int UsedCodes { get; set; }
        public int TotalQuizzes { get; set; }
        public int RecentQuizAttempts { get; set; }


        public List<RecentStudentViewModel> RecentStudents { get; set; } = new();
        public List<RecentCodeViewModel> RecentCodes { get; set; } = new();
        public List<RecentQuizViewModel> RecentQuizzes { get; set; } = new();
    }

    public class RecentStudentViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Course { get; set; } = string.Empty;
        public DateTime EnrolledAt { get; set; }
    }

    public class RecentCodeViewModel
    {
        public string Code { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }



    public class RecentQuizViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int AttemptsCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}







