using System.ComponentModel.DataAnnotations;

namespace EduPlatform.Web.ViewModels.Instructor
{
    public class GenerateCodeViewModel
    {
        [Required(ErrorMessage = "الكورس مطلوب")]
        [Display(Name = "الكورس")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "السعر مطلوب")]
        [Range(0, 100000, ErrorMessage = "السعر يجب أن يكون بين 0 و 100000")]
        [Display(Name = "السعر (جنيه)")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "الترم الدراسي مطلوب")]
        [Display(Name = "الترم الدراسي")]
        public int AcademicTermId { get; set; }

        [Range(1, 1000, ErrorMessage = "عدد الأكواد يجب أن يكون بين 1 و 1000")]
        [Display(Name = "عدد الأكواد")]
        public int Quantity { get; set; } = 1;

        [Range(0, 100, ErrorMessage = "نسبة الخصم يجب أن تكون بين 0 و 100")]
        [Display(Name = "نسبة الخصم (%)")]
        public int? DiscountPercentage { get; set; }

        [Display(Name = "ملاحظات")]
        public string? Notes { get; set; }
    }
}