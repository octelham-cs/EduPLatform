using EduPlatform.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace EduPlatform.Web.ViewModels.Admin
{
    public class SubjectListViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string NameEn { get; set; }
        public string GradeLevel { get; set; }
        public string Branch { get; set; }
        public bool IsActive { get; set; }
        public int InstructorsCount { get; set; }
        public int CoursesCount { get; set; }
    }

    public class SubjectCreateViewModel
    {
        [Required(ErrorMessage = "اسم المادة بالعربية مطلوب")]
        [Display(Name = "اسم المادة (عربي)")]
        public string Name { get; set; }

        [Display(Name = "اسم المادة (إنجليزي)")]
        public string NameEn { get; set; }

        [Required(ErrorMessage = "المستوى الدراسي مطلوب")]
        [Display(Name = "المستوى الدراسي")]
        public int GradeLevelId { get; set; }

        [Display(Name = "الشعبة (اختياري)")]
        public int? BranchId { get; set; }

        [Display(Name = "المادة نشطة")]
        public bool IsActive { get; set; } = true;

        // للـ Dropdown
        public List<GradeLevel> GradeLevels { get; set; }
        public List<Branch> Branches { get; set; }
    }

    public class SubjectEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم المادة بالعربية مطلوب")]
        [Display(Name = "اسم المادة (عربي)")]
        public string Name { get; set; }

        [Display(Name = "اسم المادة (إنجليزي)")]
        public string NameEn { get; set; }

        [Required(ErrorMessage = "المستوى الدراسي مطلوب")]
        [Display(Name = "المستوى الدراسي")]
        public int GradeLevelId { get; set; }

        [Display(Name = "الشعبة (اختياري)")]
        public int? BranchId { get; set; }

        [Display(Name = "المادة نشطة")]
        public bool IsActive { get; set; }

        // للـ Dropdown
        public List<GradeLevel> GradeLevels { get; set; }
        public List<Branch> Branches { get; set; }
    }

    public class SubjectDetailsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string NameEn { get; set; }
        public string GradeLevel { get; set; }
        public string Branch { get; set; }
        public bool IsActive { get; set; }

        // المدرسين اللي بيدرسوا المادة دي
        public List<SubjectInstructorViewModel> Instructors { get; set; }

        // الكورسات (المواد اللي بيدرسها المدرسين)
        public List<SubjectCourseViewModel> Courses { get; set; }
    }

    public class SubjectInstructorViewModel
    {
        public int InstructorId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public int StudentsCount { get; set; }
    }

    public class SubjectCourseViewModel
    {
        public int CourseId { get; set; }
        public string Title { get; set; }
        public string InstructorName { get; set; }
        public decimal Price { get; set; }
        public int StudentsCount { get; set; }
    }
}