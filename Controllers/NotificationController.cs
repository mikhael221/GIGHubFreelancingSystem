using Freelancing.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Freelancing.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
                return Unauthorized();

            var notifications = await _notificationService.GetUserNotificationsAsync(userId, 10);
            return Json(notifications);
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
                return Unauthorized();

            var count = await _notificationService.GetUnreadNotificationCountAsync(userId);
            return Json(new { count });
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(Guid notificationId)
        {
            await _notificationService.MarkNotificationAsReadAsync(notificationId);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
                return Unauthorized();

            await _notificationService.MarkAllNotificationsAsReadAsync(userId);
            return Ok();
        }
    }
}

