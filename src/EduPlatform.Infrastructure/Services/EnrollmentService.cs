using EduPlatform.Core.Entities;
using EduPlatform.Core.Enums;
using EduPlatform.Core.Interfaces;
using EduPlatform.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EduPlatform.Infrastructure.Services
{
    public class EnrollmentService : IEnrollmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EnrollmentService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<(bool Success, string Message)> ActivateCodeAsync(string code, string userId)
        {
            // 1. البحث عن الكود
            var enrollmentCode = await _context.EnrollmentCodes
                .Include(c => c.Course)
                .FirstOrDefaultAsync(c => c.Code == code);

            if (enrollmentCode == null)
                return (false, "الكود غير صحيح.");

            // 2. التحقق من حالة الكود
            if (enrollmentCode.Status != CodeStatus.Available)
                return (false, "هذا الكود مستخدم بالفعل.");

            // 3. التحقق من صلاحية الكود (تاريخ الانتهاء)
            if (enrollmentCode.EndDate < DateTime.UtcNow)
                return (false, "انتهت صلاحية هذا الكود.");

            // 4. جلب بيانات الطالب
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return (false, "المستخدم غير موجود.");

            // جلب الـ Student Record المرتبط بـ User
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);

            // لو مفيش Student Record، ننشئ واحد (ده سيناريو محتمل لو التسجيل مبينشأش Student تلقائي)
            if (student == null)
            {
                student = new Student
                {
                    UserId = userId,
                    GradeLevelId = user.GradeLevelId ?? 1, // افتراضي أو من الـ User
                    RegisteredAt = DateTime.UtcNow
                };
                _context.Students.Add(student);
                await _context.SaveChangesAsync();
            }

            // 5. إنشاء الاشتراك (Enrollment)
            var enrollment = new Enrollment
            {
                StudentId = student.Id,
                CourseId = enrollmentCode.CourseId,
                EnrollmentCodeId = enrollmentCode.Id,
                EnrolledAt = DateTime.UtcNow,
                ExpiresAt = enrollmentCode.EndDate, // مثلاً نفس تاريخ انتهاء الكود أو حسب إعدادات المادة
                Status = EnrollmentStatus.Active
            };

            // 6. تحديث حالة الكود إلى "مستخدم"
            enrollmentCode.Status = CodeStatus.Used;
            enrollmentCode.UsedAt = DateTime.UtcNow;
            enrollmentCode.UsedByStudentId = student.Id;

            // حفظ التغييرات
            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            return (true, $"تم تفعيل الاشتراك بنجاح في مادة: {enrollmentCode.Course?.Title ?? "المادة"}");
        }

        public async Task<bool> IsStudentEnrolledAsync(string userId, int courseId)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
            if (student == null) return false;

            return await _context.Enrollments
                .AnyAsync(e => e.StudentId == student.Id &&
                               e.CourseId == courseId &&
                               e.Status == EnrollmentStatus.Active &&
                               e.ExpiresAt > DateTime.UtcNow);
        }
    }
}