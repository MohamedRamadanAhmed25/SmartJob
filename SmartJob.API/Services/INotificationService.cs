using SmartJob.API.DTOs.Notifications;

namespace SmartJob.API.Services;

public interface INotificationService
{
    Task<List<NotificationDto>> GetMyNotificationsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task MarkAsReadAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default);
    Task DeleteNotificationAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
}
