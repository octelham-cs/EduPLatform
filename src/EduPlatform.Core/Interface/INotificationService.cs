using System.Collections.Generic;
using System.Threading.Tasks;
using EduPlatform.Core.Entities;

namespace EduPlatform.Core.Interfaces
{
    public interface INotificationService
    {
        Task SendNotificationAsync(string userId, string title, string message, string url = null);
        Task<List<Notification>> GetUserNotificationsAsync(string userId);
        Task<int> GetUnreadCountAsync(string userId);
        Task MarkAsReadAsync(int notificationId);
    }
}