using System.Collections.Generic;
using EduPlatform.Core.Entities;

namespace EduPlatform.Web.ViewModels.Student
{
    public class TakeQuizViewModel
    {
        public int QuizId { get; set; }
        public string Title { get; set; }
        public int TimeLimit { get; set; } // دقائق

        // الأسئلة (هنمرر الـ Entity عادي أو نعمل Map)
        public List<Question> Questions { get; set; }
    }

    public class SubmitQuizViewModel
    {
        public int QuizId { get; set; }
        // Dictionary للإجابات: Key = QuestionId, Value = Answer
        public Dictionary<int, string> Answers { get; set; }
    }
}