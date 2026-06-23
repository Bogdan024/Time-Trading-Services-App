using API.Entities;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class NotificationRepository(AppDbContext context) : INotificationRepository
{
    public async Task<IReadOnlyList<Notification>> GetNotificationsForMemberAsync(string memberId, bool unreadOnly = false)
    {
        var query = context.Notifications
            .Where(x => x.RecipientMemberId == memberId)
            .AsQueryable();

        if (unreadOnly)
        {
            query = query.Where(x => x.ReadAtUtc == null);
        }

        return await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(30)
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountForMemberAsync(string memberId)
    {
        return await context.Notifications.CountAsync(x => x.RecipientMemberId == memberId && x.ReadAtUtc == null);
    }

    public async Task<Notification?> GetNotificationForMemberAsync(int id, string memberId)
    {
        return await context.Notifications.SingleOrDefaultAsync(x => x.Id == id && x.RecipientMemberId == memberId);
    }

    public async Task MarkAllAsReadAsync(string memberId)
    {
        await context.Notifications
            .Where(x => x.RecipientMemberId == memberId && x.ReadAtUtc == null)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.ReadAtUtc, DateTime.UtcNow));
    }

    public async Task<bool> SaveAllAsync()
    {
        return await context.SaveChangesAsync() > 0;
    }
}
