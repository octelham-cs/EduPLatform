namespace EduPlatform.Web.ViewModels.Student
{
    public class QuizResultViewModel
    {
        public int QuizId { get; set; }
        public string QuizTitle { get; set; }
        public int Score { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public int WrongAnswers { get; set; }
        public int PassingScore { get; set; }
        public bool Passed { get; set; }
        public int TimeSpent { get; set; } // بالثواني
        public int AttemptNumber { get; set; }
        public int RemainingAttempts { get; set; }

        public int? NextVideoId { get; set; }
        public int? CourseId { get; set; }

        public List<QuestionReviewViewModel> QuestionReviews { get; set; }
    }

    public class QuestionReviewViewModel
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public string UserAnswer { get; set; }
        public string CorrectAnswer { get; set; }
        public bool IsCorrect { get; set; }
        public string Explanation { get; set; }
    }
}