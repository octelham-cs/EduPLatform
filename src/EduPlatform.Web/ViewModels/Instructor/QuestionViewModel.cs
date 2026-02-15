using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EduPlatform.Core.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EduPlatform.Web.ViewModels.Instructor
{
    public class QuestionViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "نص السؤال مطلوب")]
        public string QuestionText { get; set; }

        [Required(ErrorMessage = "نوع السؤال مطلوب")]
        public QuestionType Type { get; set; }

        [Required(ErrorMessage = "المادة مطلوبة")]
        public int SubjectId { get; set; }

        public int? ChapterId { get; set; }

        [Range(1, 5, ErrorMessage = "مستوى الصعوبة من 1 إلى 5")]
        public int DifficultyLevel { get; set; } = 1;

        // للاستخدام في الـ View
        public List<string> Options { get; set; } = new List<string> { "", "", "", "" };

        [Required(ErrorMessage = "الإجابة الصحيحة مطلوبة")]
        public string CorrectAnswer { get; set; }

        // القوائم المنسدلة
        public List<SelectListItem>? Subjects { get; set; }
        public List<SelectListItem>? Chapters { get; set; }
    }
}