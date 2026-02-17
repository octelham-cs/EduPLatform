using System.ComponentModel.DataAnnotations;

namespace EduPlatform.Web.ViewModels.Admin
{
    public class BranchListViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string NameEn { get; set; }
        public bool IsActive { get; set; }
        public int SubjectsCount { get; set; }
        public int StudentsCount { get; set; }
    }

    public class BranchCreateViewModel
    {
        [Required(ErrorMessage = "اسم الشعبة بالعربية مطلوب")]
        [Display(Name = "اسم الشعبة (عربي)")]
        public string Name { get; set; }

        [Display(Name = "اسم الشعبة (إنجليزي)")]
        public string NameEn { get; set; }

        [Display(Name = "الشعبة نشطة")]
        public bool IsActive { get; set; } = true;
    }

    public class BranchEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم الشعبة بالعربية مطلوب")]
        [Display(Name = "اسم الشعبة (عربي)")]
        public string Name { get; set; }

        [Display(Name = "اسم الشعبة (إنجليزي)")]
        public string NameEn { get; set; }

        [Display(Name = "الشعبة نشطة")]
        public bool IsActive { get; set; }
    }

    public class BranchDetailsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string NameEn { get; set; }
        public bool IsActive { get; set; }

        // المواد في هذه الشعبة
        public List<BranchSubjectViewModel> Subjects { get; set; }

        // الطلاب في هذه الشعبة
        public List<BranchStudentViewModel> Students { get; set; }
    }

    public class BranchSubjectViewModel
    {
        public int SubjectId { get; set; }
        public string SubjectName { get; set; }
        public string GradeLevel { get; set; }
        public int InstructorsCount { get; set; }
    }

    public class BranchStudentViewModel
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string Email { get; set; }
        public string GradeLevel { get; set; }
        public DateTime RegisteredAt { get; set; }
    }
}