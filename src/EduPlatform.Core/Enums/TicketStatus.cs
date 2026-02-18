namespace EduPlatform.Core.Enums
{
    public enum TicketStatus
    {
        Open = 1,           // مفتوحة - في انتظار الرد
        InProgress = 2,     // قيد المراجعة - أدمن شغال عليها
        Resolved = 3,       // تم الحل
        Closed = 4          // مغلقة
    }
}