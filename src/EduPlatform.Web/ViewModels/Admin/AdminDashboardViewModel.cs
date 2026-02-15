// src/EduPlatform.Web/ViewModels/Admin/AdminDashboardViewModel.cs

using System.Collections.Generic;

namespace EduPlatform.Web.ViewModels.Admin
{
    public class AdminDashboardViewModel
    {
        public int TotalInstructors { get; set; }
        public int PendingInstructors { get; set; }
        public int ApprovedInstructors { get; set; }
        public int TotalStudents { get; set; }
        public int TotalCourses { get; set; }
        public int TotalVideos { get; set; }
        public int TotalCodes { get; set; }

        public List<PendingInstructorViewModel> PendingInstructorsList { get; set; } = new();
    }

    public class PendingInstructorViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime RegisteredAt { get; set; }
    }
}