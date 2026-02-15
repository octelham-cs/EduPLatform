using System.ComponentModel.DataAnnotations;

namespace EduPlatform.Web.ViewModels.Instructor
{
    public class AddVideoViewModel
    {
        public int CourseId { get; set; }

        [Required(ErrorMessage = "الفصل مطلوب")]
        [Display(Name = "الفصل/القسم")]
        public int ChapterId { get; set; }

        [Required(ErrorMessage = "عنوان الفيديو مطلوب")]
        [StringLength(200, ErrorMessage = "العنوان يجب أن يكون أقل من 200 حرف")]
        [Display(Name = "عنوان الفيديو")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "وصف الفيديو")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "رابط يوتيوب مطلوب")]
        [Url(ErrorMessage = "الرجاء إدخال رابط صحيح")]
        [Display(Name = "رابط يوتيوب")]
        public string YouTubeUrl { get; set; } = string.Empty;

        [Display(Name = "ترتيب الفيديو")]
        public int Order { get; set; } = 0;
    }
}