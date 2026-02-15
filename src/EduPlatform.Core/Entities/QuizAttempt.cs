using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPlatform.Core.Entities
{
    public class QuizAttempt
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int QuizId { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public string AnswersJson { get; set; } // إجابات الطالب

        [Required]
        public int Score { get; set; } // الدرجة من 100

        public bool Passed { get; set; }

        public DateTime StartedAt { get; set; }
        public DateTime FinishedAt { get; set; }

        // Navigation
        [ForeignKey("QuizId")]
        public virtual Quiz Quiz { get; set; }

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }
    }
}