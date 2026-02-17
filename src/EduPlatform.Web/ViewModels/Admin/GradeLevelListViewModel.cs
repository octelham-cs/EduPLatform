using System.ComponentModel.DataAnnotations;

namespace EduPlatform.Web.ViewModels.Admin
{
    public class GradeLevelListViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string NameEn { get; set; }
        public int Order { get; set; }
        public bool IsActive { get; set; }
        public int SubjectsCount { get; set; }
        public int StudentsCount { get; set; }
    }

    public class GradeLevelCreateViewModel
    {
        [Required(ErrorMessage = "اسم المستوى بالعربية مطلوب")]
        [Display(Name = "اسم المستوى (عربي)")]
        public string Name { get; set; }

        [Display(Name = "اسم المستوى (إنجليزي)")]
        public string NameEn { get; set; }

        [Required(ErrorMessage = "الترتيب مطلوب")]
        [Display(Name = "الترتيب")]
        [Range(1, 100, ErrorMessage = "الترتيب يجب أن يكون بين 1 و 100")]
        public int Order { get; set; }

        [Display(Name = "المستوى نشط")]
        public bool IsActive { get; set; } = true;
    }

    public class GradeLevelEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم المستوى بالعربية مطلوب")]
        [Display(Name = "اسم المستوى (عربي)")]
        public string Name { get; set; }

        [Display(Name = "اسم المستوى (إنجليزي)")]
        public string NameEn { get; set; }

        [Required(ErrorMessage = "الترتيب مطلوب")]
        [Display(Name = "الترتيب")]
        [Range(1, 100, ErrorMessage = "الترتيب يجب أن يكون بين 1 و 100")]
        public int Order { get; set; }

        [Display(Name = "المستوى نشط")]
        public bool IsActive { get; set; }
    }

    public class GradeLevelDetailsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string NameEn { get; set; }
        public int Order { get; set; }
        public bool IsActive { get; set; }

        // المواد في هذا المستوى
        public List<GradeLevelSubjectViewModel> Subjects { get; set; }

        // الطلاب في هذا المستوى
        public List<GradeLevelStudentViewModel> Students { get; set; }
    }

    public class GradeLevelSubjectViewModel
    {
        public int SubjectId { get; set; }
        public string SubjectName { get; set; }
        public string Branch { get; set; }
        public int InstructorsCount { get; set; }
    }

    public class GradeLevelStudentViewModel
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string Email { get; set; }
        public string Branch { get; set; }
        public DateTime RegisteredAt { get; set; }
    }
}