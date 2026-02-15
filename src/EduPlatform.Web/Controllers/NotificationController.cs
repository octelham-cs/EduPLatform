using System.Threading.Tasks;
using EduPlatform.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPlatform.Web.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // جلب الإشعارات غير المقروءة
        public async Task<JsonResult> GetUnreadCount()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var count = await _notificationService.GetUnreadCountAsync(userId);
            return Json(new { count });
        }

        // تحديد إشعار كمقروء
        [HttpPost]
        public async Task MarkAsRead(int id)
        {
            await _notificationService.MarkAsReadAsync(id);
        }
    }
}