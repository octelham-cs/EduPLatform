using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPlatform.Core.Entities
{
    public class TicketReply
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TicketId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty; // اللي بيرد (طالب، مدرس، أدمن)

        [Required]
        public string Message { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(500)]
        public string? AttachmentPath { get; set; } // لو فيه صورة مرفقة مع الرد

        // Navigation Properties
        [ForeignKey("TicketId")]
        public virtual SupportTicket Ticket { get; set; } = null!;

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;
    }
}