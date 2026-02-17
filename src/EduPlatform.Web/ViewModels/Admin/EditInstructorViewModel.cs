using System.ComponentModel.DataAnnotations;

namespace EduPlatform.Web.ViewModels.Admin
{
    public class EditInstructorViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "الاسم مطلوب")]
        [Display(Name = "الاسم الكامل")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [Display(Name = "رقم الهاتف")]
        [Phone(ErrorMessage = "رقم الهاتف غير صحيح")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Display(Name = "نبذة عن المدرس")]
        public string? Bio { get; set; }

        [Required(ErrorMessage = "المادة مطلوبة")]
        [Display(Name = "المادة")]
        public int SubjectId { get; set; }

        // للـ Dropdown - تأكد من أن نوعها هو List<Subject>
        public List<EduPlatform.Core.Entities.Subject>? Subjects { get; set; }
    }
}