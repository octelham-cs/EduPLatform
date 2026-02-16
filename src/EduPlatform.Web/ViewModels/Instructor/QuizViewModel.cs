using System;
using System.Collections.Generic;

namespace EduPlatform.Web.ViewModels.Instructor
{
    public class QuizViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int TimeLimit { get; set; }
        public int PassingScore { get; set; }
        public int QuestionsCount { get; set; }
        public int AttemptsCount { get; set; }
    }

    public class QuizResultsViewModel
    {
        public int QuizId { get; set; }
        public string QuizTitle { get; set; } = string.Empty;
        public int TotalAttempts { get; set; }
        public int PassedCount { get; set; }
        public int FailedCount { get; set; }
        public double AverageScore { get; set; }
        public List<QuizAttemptViewModel> Attempts { get; set; } = new();
    }

    public class QuizAttemptViewModel
    {
        public string StudentName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
        public DateTime AttemptDate { get; set; }
        public int Score { get; set; }
        public bool Passed { get; set; }
        public int TimeTaken { get; set; }
    }
}