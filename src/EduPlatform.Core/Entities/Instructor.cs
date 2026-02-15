using EduPlatform.Core.Enums;

namespace EduPlatform.Core.Entities
{
    public class Instructor
    {
        public int Id { get; set; }

        // ربط بـ ApplicationUser
        public string UserId { get; set; } = string.Empty;

        // نبذة عن المدرس
        public string? Bio { get; set; }

        // صورة الملف الشخصي
        public string? ProfilePicture { get; set; }

        // حالة المدرس (معلق، موافق عليه، مرفوض)
        public InstructorStatus Status { get; set; } = InstructorStatus.Pending;

        // تاريخ التسجيل
        public DateTime RegisteredAt { get; set; } = DateTime.Now;

        // تاريخ الموافقة
        public DateTime? ApprovedAt { get; set; }

        // من وافق على المدرس (Admin ID)
        public string? ApprovedBy { get; set; }

        // Navigation Properties
        public ApplicationUser User { get; set; } = null!;
    }
}