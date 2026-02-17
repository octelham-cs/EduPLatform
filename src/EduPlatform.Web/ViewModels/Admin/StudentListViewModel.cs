using EduPlatform.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace EduPlatform.Web.ViewModels.Admin
{
    public class StudentListViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string GradeLevel { get; set; }
        public string Branch { get; set; }
        public DateTime RegisteredAt { get; set; }
        public bool IsActive { get; set; }
        public int EnrollmentsCount { get; set; }
    }

    public class StudentDetailsViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string GradeLevel { get; set; }
        public string Branch { get; set; }
        public DateTime RegisteredAt { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastLogin { get; set; }

        // الاشتراكات
        public List<StudentEnrollmentViewModel> Enrollments { get; set; }

        // تقدم الطالب
        public Dictionary<string, int> Progress { get; set; }
    }

    public class StudentEnrollmentViewModel
    {
        public string CourseName { get; set; }
        public string InstructorName { get; set; }
        public DateTime EnrolledAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string Status { get; set; }
        public int Progress { get; set; }
    }

    public class StudentEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "الاسم مطلوب")]
        [Display(Name = "الاسم الكامل")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; }

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [Display(Name = "رقم الهاتف")]
        [RegularExpression(@"^01[0-9]{9}$", ErrorMessage = "رقم الهاتف غير صحيح")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "المستوى الدراسي مطلوب")]
        [Display(Name = "المستوى الدراسي")]
        public int GradeLevelId { get; set; }

        [Display(Name = "الشعبة")]
        public int? BranchId { get; set; }

        [Display(Name = "حساب نشط")]
        public bool IsActive { get; set; }

        // للـ Dropdown
        public List<GradeLevel> GradeLevels { get; set; }
        public List<Branch> Branches { get; set; }
    }
}