using API.Data;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using API.SignalR;
using Microsoft.AspNetCore.SignalR;

namespace API.Services;

public class NotificationService(AppDbContext context, IHubContext<PresenceHub> presenceHub) : INotificationService
{
    public Notification Create(
        string recipientMemberId,
        NotificationType type,
        string title,
        string body,
        int? timeTaskId = null,
        int? groupId = null,
        int? conversationId = null)
    {
        var notification = new Notification
        {
            RecipientMemberId = recipientMemberId,
            Type = type,
            Title = title,
            Body = body,
            TimeTaskId = timeTaskId,
            GroupId = groupId,
            ConversationId = conversationId
        };

        context.Notifications.Add(notification);

        return notification;
    }

    public async Task SendAsync(Notification notification)
    {
        var connections = await PresenceTracker.GetConnectionsForUser(notification.RecipientMemberId);

        if (connections.Count > 0)
        {
            await presenceHub.Clients.Clients(connections).SendAsync("NewNotification", notification.ToDto());
        }
    }

    public async Task SendAsync(IEnumerable<Notification> notifications)
    {
        foreach (var notification in notifications)
        {
            await SendAsync(notification);
        }
    }
}
