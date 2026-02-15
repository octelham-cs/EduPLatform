using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPlatform.Core.Entities
{
    public class QuizAttempt
    {
        [Key]
        public int Id { get; set; }

        public int QuizId { get; set; }
        public int StudentId { get; set; }

        public string? AnswersJson { get; set; }
        public int Score { get; set; }
        public bool Passed { get; set; }

        public DateTime StartedAt { get; set; }
        public DateTime FinishedAt { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(QuizId))]
        public Quiz Quiz { get; set; } = null!;

        [ForeignKey(nameof(StudentId))]
        public Student Student { get; set; } = null!;
    }
}