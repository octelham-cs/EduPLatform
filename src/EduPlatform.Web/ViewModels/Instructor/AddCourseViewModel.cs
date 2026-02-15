using System.ComponentModel.DataAnnotations;

namespace EduPlatform.Web.ViewModels.Instructor
{
    public class AddCourseViewModel
    {
        [Required(ErrorMessage = "المادة الدراسية مطلوبة")]
        [Display(Name = "المادة الدراسية")]
        public int SubjectId { get; set; }

        [Required(ErrorMessage = "عنوان الكورس مطلوب")]
        [StringLength(200, ErrorMessage = "العنوان يجب أن يكون أقل من 200 حرف")]
        [Display(Name = "عنوان الكورس")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "وصف الكورس")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "السعر مطلوب")]
        [Range(0, 100000, ErrorMessage = "السعر يجب أن يكون بين 0 و 100000")]
        [Display(Name = "السعر (جنيه)")]
        public decimal Price { get; set; }
    }
}