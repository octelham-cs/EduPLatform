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

        [Required]
        public int CourseId { get; set; }

        public int? VideoId { get; set; } // اختياري (للاختبارات البعد فيديو)

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [Required]
        public QuizType Type { get; set; }

        [Required]
        public int TimeLimit { get; set; } // بالدقائق

        [Required]
        public int PassingScore { get; set; } // نسبة النجاح (مثلاً 60)

        public int MaxAttempts { get; set; } = 0; // 0 = غير محدود

        // هنحفظ قائمة أرقام الأسئلة هنا (JSON)
        [Required]
        public string QuestionsJson { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }
    }
}