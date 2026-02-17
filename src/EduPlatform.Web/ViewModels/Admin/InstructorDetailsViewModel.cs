using System;
using System.Collections.Generic;

namespace EduPlatform.Web.ViewModels.Admin
{
    public class InstructorDetailsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public string? ProfilePicture { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime RegisteredAt { get; set; }
        public DateTime? ApprovedAt { get; set; }

        // إحصائيات
        public int TotalCourses { get; set; }
        public int TotalStudents { get; set; }
        public int TotalVideos { get; set; }
        public int TotalQuizzes { get; set; }

        // أحدث الكورسات
        public List<InstructorCourseViewModel> RecentCourses { get; set; } = new();
    }

    public class InstructorCourseViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StudentsCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}