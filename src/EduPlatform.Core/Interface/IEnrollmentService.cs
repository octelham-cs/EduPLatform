using System.Threading.Tasks;
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
    }
}