using System.ComponentModel.DataAnnotations;

namespace EduPlatform.Web.ViewModels.Instructor
{
    public class AddChapterViewModel
    {
        public int CourseId { get; set; }

        [Required(ErrorMessage = "عنوان الفصل مطلوب")]
        [StringLength(100, ErrorMessage = "العنوان يجب أن يكون أقل من 100 حرف")]
        [Display(Name = "عنوان الفصل")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "وصف الفصل")]
        public string? Description { get; set; }

        [Display(Name = "ترتيب الفصل")]
        public int Order { get; set; } = 0;
    }
}