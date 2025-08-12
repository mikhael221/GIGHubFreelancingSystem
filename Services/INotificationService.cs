using Freelancing.Models.Entities;

namespace Freelancing.Services
{
    public interface INotificationService
    {
        Task<Notification> CreateNotificationAsync(Guid userId, string title, string message, string type, string? iconSvg = null, string? relatedUrl = null);
        Task<List<Notification>> GetUserNotificationsAsync(Guid userId, int count = 10);
        Task<int> GetUnreadNotificationCountAsync(Guid userId);
        Task MarkNotificationAsReadAsync(Guid notificationId);
        Task MarkAllNotificationsAsReadAsync(Guid userId);
    }
}

