using EduPlatform.Core.Entities;
using EduPlatform.Core.Enums;
using EduPlatform.Core.Interfaces;
using EduPlatform.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduPlatform.Infrastructure.Services
{
    public class EnrollmentService : IEnrollmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationService _notificationService;

        // Constructor واحد فقط بكل ال dependencies
        public EnrollmentService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            INotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        public async Task<(bool Success, string Message)> ActivateCodeAsync(string code, string userId)
        {
            // 1. البحث عن الكود
            var enrollmentCode = await _context.EnrollmentCodes
                .Include(c => c.Course)
                .Include(c => c.Instructor)
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

            // لو مفيش Student Record، ننشئ واحد
            if (student == null)
            {
                student = new Student
                {
                    UserId = userId,
                    GradeLevelId = user.GradeLevelId ?? 1,
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
                ExpiresAt = enrollmentCode.EndDate,
                Status = EnrollmentStatus.Active
            };

            // 6. تحديث حالة الكود إلى "مستخدم"
            enrollmentCode.Status = CodeStatus.Used;
            enrollmentCode.UsedAt = DateTime.UtcNow;
            enrollmentCode.UsedBy = student.Id;

            // حفظ التغييرات
            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            // إرسال إشعار للمدرس
            try
            {
                if (enrollmentCode.Instructor != null)
                {
                    var instructorUser = await _userManager.FindByIdAsync(enrollmentCode.Instructor.UserId);
                    if (instructorUser != null)
                    {
                        await _notificationService.SendNotificationAsync(
                            instructorUser.Id,
                            "اشتراك جديد",
                            $"قام الطالب {user.FullName} بالاشتراك في كورس {enrollmentCode.Course?.Title ?? "المادة"}.",
                            "/Instructor/Students"
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                // سجل الخطأ لكن لا تفشل العملية
                Console.WriteLine($"خطأ في إرسال الإشعار: {ex.Message}");
            }

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

        public async Task<List<Enrollment>> GetStudentEnrollmentsAsync(string userId)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
            if (student == null) return new List<Enrollment>();

            var enrollments = await _context.Enrollments
                .Include(e => e.Course)
                    .ThenInclude(c => c.Instructor)
                        .ThenInclude(i => i.User)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Subject)
                .Where(e => e.StudentId == student.Id)
                .OrderByDescending(e => e.EnrolledAt)
                .ToListAsync();

            // تحديث الحالة
            bool hasChanges = false;
            foreach (var enrollment in enrollments)
            {
                if (enrollment.Status == EnrollmentStatus.Active && enrollment.ExpiresAt < DateTime.UtcNow)
                {
                    enrollment.Status = EnrollmentStatus.Expired;
                    hasChanges = true;
                }
            }

            if (hasChanges)
            {
                await _context.SaveChangesAsync();
            }

            return enrollments;
        }

        // ====== الطرق الجديدة المضافة ======

        public async Task<List<Enrollment>> GetActiveEnrollmentsAsync(string userId)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
            if (student == null) return new List<Enrollment>();

            var today = DateTime.UtcNow.Date;

            return await _context.Enrollments
                .Include(e => e.Course)
                    .ThenInclude(c => c.Instructor)
                        .ThenInclude(i => i.User)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Subject)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Chapters)
                .Where(e => e.StudentId == student.Id
                    && e.Status == EnrollmentStatus.Active
                    && e.ExpiresAt.Date >= today)
                .OrderByDescending(e => e.EnrolledAt)
                .ToListAsync();
        }

        public async Task<List<Enrollment>> GetExpiredEnrollmentsAsync(string userId)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
            if (student == null) return new List<Enrollment>();

            var today = DateTime.UtcNow.Date;

            return await _context.Enrollments
                .Include(e => e.Course)
                    .ThenInclude(c => c.Instructor)
                        .ThenInclude(i => i.User)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Subject)
                .Where(e => e.StudentId == student.Id
                    && (e.Status == EnrollmentStatus.Expired || e.ExpiresAt.Date < today))
                .OrderByDescending(e => e.ExpiresAt)
                .ToListAsync();
        }

        public async Task<bool> IsEnrollmentValidAsync(int enrollmentId)
        {
            var enrollment = await _context.Enrollments.FindAsync(enrollmentId);
            if (enrollment == null) return false;

            if (enrollment.Status != EnrollmentStatus.Active) return false;

            if (enrollment.ExpiresAt.Date < DateTime.UtcNow.Date)
            {
                enrollment.Status = EnrollmentStatus.Expired;
                await _context.SaveChangesAsync();
                return false;
            }

            return true;
        }

        public async Task<(bool Success, string Message)> RenewEnrollmentAsync(int oldEnrollmentId, string newCode)
        {
            var oldEnrollment = await _context.Enrollments.FindAsync(oldEnrollmentId);
            if (oldEnrollment == null)
                return (false, "الاشتراك غير موجود");

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == oldEnrollment.StudentId);

            if (student == null)
                return (false, "الطالب غير موجود");

            // تفعيل الكود الجديد
            var result = await ActivateCodeAsync(newCode, student.UserId);

            if (result.Success)
            {
                // تحديث حالة الاشتراك القديم
                oldEnrollment.Status = EnrollmentStatus.Expired;
                await _context.SaveChangesAsync();
            }

            return result;
        }

        public async Task CheckExpiredEnrollmentsAsync()
        {
            var today = DateTime.UtcNow.Date;

            var expiredEnrollments = await _context.Enrollments
                .Where(e => e.Status == EnrollmentStatus.Active && e.ExpiresAt.Date < today)
                .ToListAsync();

            foreach (var enrollment in expiredEnrollments)
            {
                enrollment.Status = EnrollmentStatus.Expired;
            }

            if (expiredEnrollments.Any())
            {
                await _context.SaveChangesAsync();
            }
        }
    }
}