using System.ComponentModel.DataAnnotations;

namespace EduPlatform.Web.ViewModels.Admin
{
    public class AssignInstructorViewModel
    {
        [Required(ErrorMessage = "يجب اختيار مستخدم")]
        [Display(Name = "المستخدم")]
        public string UserId { get; set; } = string.Empty;

        [Display(Name = "السيرة الذاتية")]
        public string? Bio { get; set; }

        [Required(ErrorMessage = "يجب اختيار مادة للمدرس")]
        [Display(Name = "المادة")]
        public int SubjectId { get; set; }
    }

    public class CreateInstructorViewModel
    {
        [Required(ErrorMessage = "الاسم الكامل مطلوب")]
        [StringLength(100, ErrorMessage = "الاسم يجب أن يكون أقل من 100 حرف")]
        [Display(Name = "الاسم الكامل")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [Phone(ErrorMessage = "رقم الهاتف غير صحيح")]
        [Display(Name = "رقم الهاتف")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Display(Name = "السيرة الذاتية")]
        public string? Bio { get; set; }

        [Required(ErrorMessage = "يجب اختيار مادة للمدرس")]
        [Display(Name = "المادة")]
        public int SubjectId { get; set; }
    }
}