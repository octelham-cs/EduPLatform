using System.ComponentModel.DataAnnotations;

namespace EduPlatform.Web.ViewModels.Account
{
    public class ProfileViewModel
    {
        [Required(ErrorMessage = "الاسم الكامل مطلوب")]
        [StringLength(100, ErrorMessage = "الاسم يجب أن يكون أقل من 100 حرف")]
        [Display(Name = "الاسم الكامل")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "رقم الهاتف غير صحيح")]
        [Display(Name = "رقم الهاتف")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Display(Name = "تاريخ التسجيل")]
        public DateTime CreatedAt { get; set; }

        // حقول إضافية للمدرسين
        [Display(Name = "السيرة الذاتية")]
        public string? Bio { get; set; }

        [Display(Name = "المؤهلات")]
        public string? Qualifications { get; set; }
    }
}