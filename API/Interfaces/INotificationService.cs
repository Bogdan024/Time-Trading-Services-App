using API.Entities;

namespace API.Interfaces;

public interface INotificationService
{
    Notification Create(
        string recipientMemberId,
        NotificationType type,
        string title,
        string body,
        int? timeTaskId = null,
        int? groupId = null,
        int? conversationId = null);

    Task SendAsync(Notification notification);
    Task SendAsync(IEnumerable<Notification> notifications);
}
