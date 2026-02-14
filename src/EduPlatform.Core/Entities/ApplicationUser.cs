using Microsoft.AspNetCore.Identity;
using System;

namespace EduPlatform.Core.Entities
{
    public class ApplicationUser : IdentityUser
    {
        // الاسم الكامل للمستخدم
        public string FullName { get; set; } = string.Empty;

        // تاريخ إنشاء الحساب
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // آخر تسجيل دخول
        public DateTime? LastLogin { get; set; }

        // هل الحساب نشط؟
        public bool IsActive { get; set; } = true;

        // المستوى الدراسي (للطالب فقط)
        public int? GradeLevelId { get; set; }

        // الشعبة (للطالب فقط)
        public int? BranchId { get; set; }
    }
}