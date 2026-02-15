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

        [Required]
        public int SubjectId { get; set; }

        public int? ChapterId { get; set; }

        [Required]
        public QuestionType Type { get; set; }

        [Required]
        public string QuestionText { get; set; }

        // حفظ الاختيارات بصيغة JSON
        public string? OptionsJson { get; set; }

        // حفظ الإجابة الصحيحة
        [Required]
        public string CorrectAnswerJson { get; set; }

        public int DifficultyLevel { get; set; } = 1; // 1 إلى 5

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey("InstructorId")]
        public virtual Instructor Instructor { get; set; }

        [ForeignKey("SubjectId")]
        public virtual Subject Subject { get; set; }
    }
}