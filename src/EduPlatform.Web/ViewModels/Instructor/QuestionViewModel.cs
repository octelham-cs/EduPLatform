// src/EduPlatform.Web/ViewModels/Instructor/AddQuestionViewModel.cs

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EduPlatform.Web.ViewModels.Instructor
{
    public class AddQuestionViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "نص السؤال مطلوب")]
        [Display(Name = "نص السؤال")]
        public string QuestionText { get; set; } = string.Empty;

        [Required(ErrorMessage = "يجب اختيار نوع السؤال")]
        [Display(Name = "نوع السؤال")]
        public string QuestionType { get; set; } = "MultipleChoice";

        [Display(Name = "المادة")]
        public int SubjectId { get; set; }

        [Display(Name = "الفصل")]
        public int? ChapterId { get; set; }

        [Range(1, 5, ErrorMessage = "مستوى الصعوبة بين 1 و 5")]
        [Display(Name = "مستوى الصعوبة")]
        public int DifficultyLevel { get; set; } = 3;

        // للاختيارات
        [Display(Name = "الاختيارات")]
        public List<string> Options { get; set; } = new List<string> { "", "", "", "" };

        [Display(Name = "الإجابة الصحيحة")]
        public int CorrectAnswerIndex { get; set; }

        [Display(Name = "الإجابة الصحيحة (للصح/خطأ)")]
        public bool CorrectAnswerBool { get; set; }
    }
}