using API.DTOs;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class NotificationsController(INotificationRepository notificationRepository) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<NotificationDto>>> GetNotifications([FromQuery] bool unreadOnly = false)
    {
        var notifications = await notificationRepository.GetNotificationsForMemberAsync(User.GetMemberId(), unreadOnly);

        return Ok(notifications.Select(x => x.ToDto()));
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<int>> GetUnreadCount()
    {
        return Ok(await notificationRepository.GetUnreadCountForMemberAsync(User.GetMemberId()));
    }

    [HttpPut("{id:int}/read")]
    public async Task<ActionResult> MarkAsRead(int id)
    {
        var notification = await notificationRepository.GetNotificationForMemberAsync(id, User.GetMemberId());

        if (notification is null) return NotFound();

        notification.ReadAtUtc ??= DateTime.UtcNow;

        if (await notificationRepository.SaveAllAsync()) return NoContent();

        return BadRequest("Failed to mark notification as read");
    }

    [HttpPut("read-all")]
    public async Task<ActionResult> MarkAllAsRead()
    {
        await notificationRepository.MarkAllAsReadAsync(User.GetMemberId());

        return NoContent();
    }
}
