// src/EduPlatform.Web/ViewModels/Instructor/GenerateCodeViewModel.cs

using System;
using System.ComponentModel.DataAnnotations;

namespace EduPlatform.Web.ViewModels.Instructor
{
    public class GenerateCodeViewModel
    {
        [Required(ErrorMessage = "يجب اختيار الكورس")]
        [Display(Name = "الكورس")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "يجب تحديد السعر")]
        [Range(0.01, 100000, ErrorMessage = "السعر يجب أن يكون بين 0.01 و 100000")]
        [Display(Name = "السعر")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "يجب اختيار الترم الدراسي")]
        [Display(Name = "الترم الدراسي")]
        public int AcademicTermId { get; set; }

        [Range(1, 1000, ErrorMessage = "عدد الأكواد يجب أن يكون بين 1 و 1000")]
        [Display(Name = "عدد الأكواد")]
        public int Quantity { get; set; } = 1;

        [Range(0, 100, ErrorMessage = "نسبة الخصم يجب أن تكون بين 0 و 100")]
        [Display(Name = "نسبة الخصم")]
        public int? DiscountPercentage { get; set; }

        [Required(ErrorMessage = "يجب تحديد تاريخ البداية")]
        [Display(Name = "تاريخ البداية")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "يجب تحديد تاريخ النهاية")]
        [Display(Name = "تاريخ النهاية")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Display(Name = "ملاحظات")]
        [StringLength(500)]
        public string? Notes { get; set; }
    }

    public class GeneratedCodeViewModel
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? StudentName { get; set; }
        public DateTime? UsedAt { get; set; }
    }
}