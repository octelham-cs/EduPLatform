using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EduPlatform.Core.Enums; // <--- ضفنا السطر ده

namespace EduPlatform.Core.Entities
{
    public class Enrollment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        public int EnrollmentCodeId { get; set; }

        [Required]
        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime ExpiresAt { get; set; }

        // هنا هيبقى صح دلوقتي لأنه شايف الـ Enum
        public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Active;

        // Navigation Properties
        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }

        [ForeignKey("EnrollmentCodeId")]
        public virtual EnrollmentCode EnrollmentCode { get; set; }
    }
}