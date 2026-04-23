using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartJob.API.DTOs.Notifications;
using SmartJob.API.DTOs.Jobs;
using SmartJob.API.Services;
using SmartJob.API.Models;

namespace SmartJob.API.Controllers;

/// <summary>
/// Manages user notifications and acknowledgment.
/// </summary>
[Authorize]
[ApiController]
[Route("api/notifications")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ICurrentUserService _currentUserService;

    public NotificationsController(INotificationService notificationService, ICurrentUserService currentUserService)
    {
        _notificationService = notificationService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Retrieves a paginated list of notifications for the current user.
    /// </summary>
    /// <param name="page">Page number.</param>
    /// <param name="pageSize">Items per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated list of notifications.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<NotificationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<NotificationDto>>> GetNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        return Ok(await _notificationService.GetMyNotificationsAsync(_currentUserService.GetRequiredUserId(), cancellationToken));
    }

    /// <summary>
    /// Marks a specific notification as read.
    /// </summary>
    /// <param name="id">The unique identifier of the notification.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [HttpPatch("{id}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
    {
        await _notificationService.MarkAsReadAsync(id, _currentUserService.GetRequiredUserId(), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Marks all notifications for the current user as read.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    [HttpPatch("read-all")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken)
    {
        await _notificationService.MarkAllAsReadAsync(_currentUserService.GetRequiredUserId(), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Deletes a specific notification for the current user.
    /// </summary>
    /// <param name="id">The unique identifier of the notification.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteNotification(Guid id, CancellationToken cancellationToken)
    {
        await _notificationService.DeleteNotificationAsync(id, _currentUserService.GetRequiredUserId(), cancellationToken);
        return NoContent();
    }
}
