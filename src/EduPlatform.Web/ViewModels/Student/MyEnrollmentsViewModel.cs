using System;
using System.Collections.Generic;

namespace EduPlatform.Web.ViewModels.Student
{
    public class MyEnrollmentsViewModel
    {
        public List<EnrollmentItemViewModel> Enrollments { get; set; }
    }

    public class EnrollmentItemViewModel
    {
        public int EnrollmentId { get; set; }
        public int CourseId { get; set; }
        public string CourseTitle { get; set; }
        public string InstructorName { get; set; }
        public DateTime EnrolledAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string Status { get; set; } // "نشط", "منتهي"
        public int DaysRemaining { get; set; }
        public bool IsExpired { get; set; }
    }
}