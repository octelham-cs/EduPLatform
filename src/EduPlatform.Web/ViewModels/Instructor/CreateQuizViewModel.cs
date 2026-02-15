using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EduPlatform.Core.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EduPlatform.Web.ViewModels.Instructor
{
    public class CreateQuizViewModel
    {
        public int Id { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public QuizType Type { get; set; }

        [Required]
        [Range(1, 180)]
        public int TimeLimit { get; set; } // دقائق

        [Required]
        [Range(1, 100)]
        public int PassingScore { get; set; } = 60;

        // قائمة الأسئلة المختارة
        public List<int> SelectedQuestionIds { get; set; } = new List<int>();

        // لعرض القوائم
        public List<SelectListItem> Courses { get; set; }
        public List<QuestionListItem> AvailableQuestions { get; set; }
    }

    public class QuestionListItem
    {
        public int Id { get; set; }
        public string QuestionText { get; set; }
        public string SubjectName { get; set; }
    }
}