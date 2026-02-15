using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EduPlatform.Core.Enums;

namespace EduPlatform.Core.Entities
{
    public class Quiz
    {
        [Key]
        public int Id { get; set; }

        public int CourseId { get; set; }
        public int? VideoId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        public QuizType Type { get; set; } = QuizType.Progress;
        public int TimeLimit { get; set; } = 30; // بالدقائق
        public int PassingScore { get; set; } = 60; // نسبة مئوية
        public int MaxAttempts { get; set; } = 0; // 0 = unlimited

        public string? QuestionsJson { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey(nameof(CourseId))]
        public Course Course { get; set; } = null!;

        [ForeignKey(nameof(VideoId))]
        public Video? Video { get; set; }

        public ICollection<QuizAttempt> QuizAttempts { get; set; } = new List<QuizAttempt>();
    }
}