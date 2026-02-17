using System.ComponentModel.DataAnnotations;

namespace EduPlatform.Web.ViewModels.Student
{
    public class ActivateCodeViewModel
    {
        [Required(ErrorMessage = "الكود مطلوب")]
        [Display(Name = "كود التفعيل")]
        [StringLength(20, MinimumLength = 8, ErrorMessage = "الكود يجب أن يكون بين 8 و 20 حرف")]
        public string Code { get; set; }
    }

    public class ActivateCodeResultViewModel
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string CourseName { get; set; }
        public string InstructorName { get; set; }
        public DateTime ExpiresAt { get; set; }
        public int CourseId { get; set; }
    }
}