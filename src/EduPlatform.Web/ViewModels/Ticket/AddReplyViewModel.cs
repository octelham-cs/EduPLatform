using System.ComponentModel.DataAnnotations;

namespace EduPlatform.Web.ViewModels.Ticket
{
    public class AddReplyViewModel
    {
        [Required(ErrorMessage = "الرد مطلوب")]
        [StringLength(2000, ErrorMessage = "الرد لا يزيد عن 2000 حرف")]
        [Display(Name = "الرد")]
        public string Message { get; set; } = string.Empty;

        [Display(Name = "مرفق")]
        public IFormFile? Attachment { get; set; }
    }
}