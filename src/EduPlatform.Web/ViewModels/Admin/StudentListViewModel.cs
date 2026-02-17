using EduPlatform.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace EduPlatform.Web.ViewModels.Admin
{
    public class StudentListViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string GradeLevel { get; set; } = string.Empty;
        public string Branch { get; set; } = string.Empty;
        public DateTime RegisteredAt { get; set; }
        public bool IsActive { get; set; }
        public int EnrollmentsCount { get; set; }
    }

    public class StudentDetailsViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string GradeLevel { get; set; } = string.Empty;
        public string Branch { get; set; } = string.Empty;
        public DateTime RegisteredAt { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastLogin { get; set; }
        public List<StudentEnrollmentViewModel> Enrollments { get; set; } = new();
        public Dictionary<string, int> Progress { get; set; } = new();
    }

    public class StudentEnrollmentViewModel
    {
        public string CourseName { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;
        public DateTime EnrolledAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public int Progress { get; set; }
    }

    public class StudentEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "الاسم الكامل مطلوب")]
        [Display(Name = "الاسم الكامل")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "رقم الهاتف")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "المستوى الدراسي")]
        public int? GradeLevelId { get; set; }

        [Display(Name = "الشعبة")]
        public int? BranchId { get; set; }

        [Display(Name = "حساب نشط")]
        public bool IsActive { get; set; }

        public List<GradeLevel> GradeLevels { get; set; } = new();
        public List<Branch> Branches { get; set; } = new();
    }
}