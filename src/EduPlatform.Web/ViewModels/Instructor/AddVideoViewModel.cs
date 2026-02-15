// src/EduPlatform.Web/ViewModels/Instructor/AddVideoViewModel.cs

using System;
using System.ComponentModel.DataAnnotations;

namespace EduPlatform.Web.ViewModels.Instructor
{
    public class AddVideoViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "يجب اختيار الفصل")]
        [Display(Name = "الفصل")]
        public int ChapterId { get; set; }

        [Required(ErrorMessage = "عنوان الفيديو مطلوب")]
        [StringLength(200)]
        [Display(Name = "عنوان الفيديو")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        [Display(Name = "وصف الفيديو")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "رابط YouTube مطلوب")]
        [StringLength(500)]
        [Display(Name = "رابط YouTube")]
        public string YouTubeUrl { get; set; } = string.Empty;

        [Display(Name = "ترتيب الفيديو")]
        public int Order { get; set; } = 1;

        [Display(Name = "إخفاء الفيديو")]
        public bool IsHidden { get; set; }

        // للعرض فقط
        public int CourseId { get; set; }
        public string? CourseName { get; set; }
    }
}