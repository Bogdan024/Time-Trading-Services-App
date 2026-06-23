using API.Entities;

namespace API.Interfaces;

public interface INotificationRepository
{
    Task<IReadOnlyList<Notification>> GetNotificationsForMemberAsync(string memberId, bool unreadOnly = false);
    Task<int> GetUnreadCountForMemberAsync(string memberId);
    Task<Notification?> GetNotificationForMemberAsync(int id, string memberId);
    Task MarkAllAsReadAsync(string memberId);
    Task<bool> SaveAllAsync();
}
