using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartJob.API.Data;
using SmartJob.API.DTOs.Notifications;
using SmartJob.API.Exceptions;

namespace SmartJob.API.Services;

public class NotificationService : INotificationService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;

    public NotificationService(AppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<List<NotificationDto>> GetMyNotificationsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var notifications = await _db.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(50) // reasonable limit
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<NotificationDto>>(notifications);
    }

    public async Task MarkAsReadAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var notif = await _db.Notifications.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId, cancellationToken)
            ?? throw new ApiException(StatusCodes.Status404NotFound, "Notification not found.");

        notif.IsRead = true;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var unread = await _db.Notifications.Where(n => n.UserId == userId && !n.IsRead).ToListAsync(cancellationToken);
        foreach (var notif in unread)
        {
            notif.IsRead = true;
        }
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteNotificationAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var notif = await _db.Notifications.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId, cancellationToken)
            ?? throw new ApiException(StatusCodes.Status404NotFound, "Notification not found.");

        _db.Notifications.Remove(notif);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
