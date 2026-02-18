namespace EduPlatform.Web.ViewModels.Ticket
{
    public class TicketListItemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusColor { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string PriorityColor { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public string UserType { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string CreatedAtFormatted => CreatedAt.ToString("yyyy/MM/dd HH:mm");
        public int RepliesCount { get; set; }
        public bool IsClosed => Status == "مغلقة" || Status == "تم الحل";

        public string CreatedByEmail { get; internal set; }
    }
}