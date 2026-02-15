using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPlatform.Core.Entities
{
    public class Student
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public int GradeLevelId { get; set; }
        public int? BranchId { get; set; }

        public DateTime RegisteredAt { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = null!;

        [ForeignKey(nameof(GradeLevelId))]
        public GradeLevel GradeLevel { get; set; } = null!;

        [ForeignKey(nameof(BranchId))]
        public Branch? Branch { get; set; }

        // Collections
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public ICollection<EnrollmentCode> EnrollmentCodes { get; set; } = new List<EnrollmentCode>();
    }
}