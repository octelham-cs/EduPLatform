using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace EduPlatform.Web.Hubs
{
    [Authorize] // لازم يكون مسجل دخول
    public class NotificationHub : Hub
    {
        // هذه الدالة تعمل تلقائياً لما المستخدم يعمل Connect
        public override async Task OnConnectedAsync()
        {
            // بنضيف المستخدم لـ Group خاصة بيه (باستخدام الـ UserId)
            var userId = Context.UserIdentifier;
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);

            await base.OnConnectedAsync();
        }
    }
}