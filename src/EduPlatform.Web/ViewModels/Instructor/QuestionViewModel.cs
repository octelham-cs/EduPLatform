using System.ComponentModel.DataAnnotations;

namespace EduPlatform.Web.ViewModels.Instructor
{
    public class AddQuestionViewModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "نص السؤال مطلوب")]
        [Display(Name = "نص السؤال")]
        public string QuestionText { get; set; } = string.Empty;

        [Required(ErrorMessage = "نوع السؤال مطلوب")]
        [Display(Name = "نوع السؤال")]
        public string QuestionType { get; set; } = string.Empty;

        [Required(ErrorMessage = "المادة مطلوبة")]
        [Display(Name = "المادة")]
        public int SubjectId { get; set; }

        [Display(Name = "الفصل")]
        public int? ChapterId { get; set; }

        [Range(1, 5, ErrorMessage = "مستوى الصعوبة يجب أن يكون بين 1 و 5")]
        [Display(Name = "مستوى الصعوبة")]
        public int DifficultyLevel { get; set; } = 3;

        // للاختيارات
        public List<string> Options { get; set; } = new List<string> { "", "", "", "" };

        // للإجابة الصحيحة
        public int? CorrectAnswerIndex { get; set; }
        public bool? CorrectAnswerBool { get; set; }
        public List<int> CorrectAnswerIndices { get; set; } = new List<int>();
    }
}