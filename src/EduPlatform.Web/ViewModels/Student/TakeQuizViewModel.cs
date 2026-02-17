using System.ComponentModel.DataAnnotations;

namespace EduPlatform.Web.ViewModels.Student
{
    public class TakeQuizViewModel
    {
        public int QuizId { get; set; }
        public string QuizTitle { get; set; }
        public string CourseName { get; set; }
        public int TimeLimit { get; set; } // بالدقائق
        public int PassingScore { get; set; }
        public int MaxAttempts { get; set; }
        public int CurrentAttempt { get; set; }
        public List<QuizQuestionViewModel> Questions { get; set; }
    }

    public class QuizQuestionViewModel
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public string QuestionType { get; set; } // MultipleChoice, TrueFalse, etc.
        public List<QuestionOptionViewModel> Options { get; set; }
        public int Order { get; set; }
    }

    public class QuestionOptionViewModel
    {
        public string Id { get; set; }
        public string Text { get; set; }
    }

    public class SubmitQuizViewModel
    {
        public int QuizId { get; set; }
        public int TimeSpent { get; set; } // بالثواني
        public Dictionary<int, string> Answers { get; set; } // QuestionId -> Answer
    }
}