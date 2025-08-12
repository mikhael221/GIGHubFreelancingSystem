using Freelancing.Data;
using Freelancing.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Freelancing.Hubs;

namespace Freelancing.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<MentorshipChatHub> _hubContext;

        public NotificationService(ApplicationDbContext context, IHubContext<MentorshipChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<Notification> CreateNotificationAsync(Guid userId, string title, string message, string type, string? iconSvg = null, string? relatedUrl = null)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                IconSvg = iconSvg,
                RelatedUrl = relatedUrl,
                CreatedAt = DateTime.Now,
                IsRead = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Broadcast the notification in real-time
            await MentorshipChatHub.BroadcastNotification(_hubContext, notification);

            // Update notification count
            var unreadCount = await GetUnreadNotificationCountAsync(userId);
            await MentorshipChatHub.UpdateNotificationCount(_hubContext, userId, unreadCount);

            return notification;
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(Guid userId, int count = 10)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<int> GetUnreadNotificationCountAsync(Guid userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task MarkNotificationAsReadAsync(Guid notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.Now;
                await _context.SaveChangesAsync();

                // Update notification count in real-time
                var unreadCount = await GetUnreadNotificationCountAsync(notification.UserId);
                await MentorshipChatHub.UpdateNotificationCount(_hubContext, notification.UserId, unreadCount);
            }
        }

        public async Task MarkAllNotificationsAsReadAsync(Guid userId)
        {
            var unreadNotifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            // Update notification count in real-time
            var unreadCount = await GetUnreadNotificationCountAsync(userId);
            await MentorshipChatHub.UpdateNotificationCount(_hubContext, userId, unreadCount);
        }
    }
}

