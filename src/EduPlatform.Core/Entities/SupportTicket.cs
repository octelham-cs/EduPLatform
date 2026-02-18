using EduPlatform.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Sockets;
using EduPlatform.Core.Entities;

namespace EduPlatform.Core.Entities
{
    public class SupportTicket
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string CreatedById { get; set; } = string.Empty; // UserId (Student or Instructor)

        [Required]
        public TicketType UserType { get; set; } // Student or Instructor

        [Required]
        public TicketStatus Status { get; set; } = TicketStatus.Open;

        [Required]
        public TicketPriority Priority { get; set; } = TicketPriority.Medium;

        public int? AssignedToAdminId { get; set; } // Admin Id لو اتخصص لأدمن معين

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public DateTime? ClosedAt { get; set; }

        [StringLength(500)]
        public string? AttachmentPath { get; set; } // لو فيه صورة مرفقة

        // Navigation Properties
        [ForeignKey("CreatedById")]
        public virtual ApplicationUser CreatedBy { get; set; } = null!;

        [ForeignKey("AssignedToAdminId")]
        //public virtual Admin? AssignedToAdmin { get; set; }

        public virtual ICollection<TicketReply> Replies { get; set; } = new List<TicketReply>();
    }
}