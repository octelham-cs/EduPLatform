using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace EduPlatform.Web.ViewModels.Ticket
{
    public class CreateTicketViewModel
    {
        [Required(ErrorMessage = "عنوان التذكرة مطلوب")]
        [StringLength(100, ErrorMessage = "العنوان لا يزيد عن 100 حرف")]
        [Display(Name = "العنوان")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "الوصف مطلوب")]
        [StringLength(2000, ErrorMessage = "الوصف لا يزيد عن 2000 حرف")]
        [Display(Name = "الوصف")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "الأولوية")]
        public int Priority { get; set; } = 2; // Medium default

        [Display(Name = "مرفق")]
        public IFormFile? Attachment { get; set; }

        public List<SelectListItem> Priorities { get; set; } = new();
    }
}