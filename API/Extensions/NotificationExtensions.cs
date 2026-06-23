using API.DTOs;
using API.Entities;

namespace API.Extensions;

public static class NotificationExtensions
{
    public static NotificationDto ToDto(this Notification notification)
    {
        return new NotificationDto
        {
            Id = notification.Id,
            Type = notification.Type,
            Title = notification.Title,
            Body = notification.Body,
            TimeTaskId = notification.TimeTaskId,
            GroupId = notification.GroupId,
            ConversationId = notification.ConversationId,
            CreatedAtUtc = notification.CreatedAtUtc,
            ReadAtUtc = notification.ReadAtUtc
        };
    }
}
