using EduPlatform.Core.Entities;

namespace EduPlatform.Web.ViewModels.Ticket
{
    public class TicketDetailsViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusColor { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string PriorityColor { get; set; } = string.Empty;
        public string CreatedByName { get; set; } = string.Empty;
        public string CreatedByEmail { get; set; } = string.Empty;
        public string UserType { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string CreatedAtFormatted => CreatedAt.ToString("yyyy/MM/dd HH:mm");
        public DateTime? ClosedAt { get; set; }
        public string? AttachmentPath { get; set; }
        public string? AttachmentFileName => string.IsNullOrEmpty(AttachmentPath) ? null : Path.GetFileName(AttachmentPath);

        public List<TicketReplyViewModel> Replies { get; set; } = new();

        // للردود
        public class TicketReplyViewModel
        {
            public int Id { get; set; }
            public string Message { get; set; } = string.Empty;
            public string UserName { get; set; } = string.Empty;
            public string UserRole { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
            public string CreatedAtFormatted => CreatedAt.ToString("yyyy/MM/dd HH:mm");
            public string? AttachmentPath { get; set; }
            public string? AttachmentFileName => string.IsNullOrEmpty(AttachmentPath) ? null : Path.GetFileName(AttachmentPath);
            public bool IsAdmin => UserRole == "Admin";
            public bool IsInstructor => UserRole == "Instructor";
            public bool IsStudent => UserRole == "Student";
        }
    }
}