using EduPlatform.Core.Entities;

namespace EduPlatform.Core.Interfaces
{
    public interface IEnrollmentService
    {
        /// <summary>
        /// تفعيل كود الاشتراك للطالب
        /// </summary>
        Task<(bool Success, string Message)> ActivateCodeAsync(string code, string userId);

        /// <summary>
        /// التحقق هل الطالب مشترك في مادة معينة
        /// </summary>
        Task<bool> IsStudentEnrolledAsync(string userId, int courseId);


        Task<List<Enrollment>> GetStudentEnrollmentsAsync(string userId);





        Task<List<Enrollment>> GetActiveEnrollmentsAsync(string userId);

        /// <summary>
        /// جلب الاشتراكات المنتهية فقط
        /// </summary>
        Task<List<Enrollment>> GetExpiredEnrollmentsAsync(string userId);

        /// <summary>
        /// التحقق من صلاحية اشتراك معين
        /// </summary>
        Task<bool> IsEnrollmentValidAsync(int enrollmentId);

        /// <summary>
        /// تجديد اشتراك منتهي
        /// </summary>
        Task<(bool Success, string Message)> RenewEnrollmentAsync(int oldEnrollmentId, string newCode);

        /// <summary>
        /// فحص الاشتراكات المنتهية (للتشغيل التلقائي)
        /// </summary>
        Task CheckExpiredEnrollmentsAsync();
    }
}