using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EduPlatform.Core.Enums;

namespace EduPlatform.Core.Entities
{
    public class Question
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int InstructorId { get; set; }

        public int SubjectId { get; set; }

        public int? ChapterId { get; set; }

        [Required]
        public QuestionType Type { get; set; }

        [Required]
        [StringLength(2000)]
        public string QuestionText { get; set; } = string.Empty;

        public string? OptionsJson { get; set; }
        public string? CorrectAnswerJson { get; set; }

        [Range(1, 5)]
        public int DifficultyLevel { get; set; } = 3;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey(nameof(InstructorId))]
        public Instructor Instructor { get; set; } = null!;

        [ForeignKey(nameof(SubjectId))]
        public Subject Subject { get; set; } = null!;

        [ForeignKey(nameof(ChapterId))]
        public Chapter? Chapter { get; set; }
    }
}