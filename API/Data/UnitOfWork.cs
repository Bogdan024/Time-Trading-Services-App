using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    private IMemberRepository? memberRepository;
    private ITimeTaskRepository? timeTaskRepository;
    private IReviewRepository? reviewRepository;
    private IMessageRepository? messageRepository;
    private IGroupRepository? groupRepository;
    private IModerationRepository? moderationRepository;
    private INotificationRepository? notificationRepository;

    public IMemberRepository MemberRepository => memberRepository ??= new MemberRepository(context);
    public ITimeTaskRepository TimeTaskRepository => timeTaskRepository ??= new TimeTaskRepository(context);
    public IReviewRepository ReviewRepository => reviewRepository ??= new ReviewRepository(context);
    public IMessageRepository MessageRepository => messageRepository ??= new MessageRepository(context);
    public IGroupRepository GroupRepository => groupRepository ??= new GroupRepository(context);
    public IModerationRepository ModerationRepository => moderationRepository ??= new ModerationRepository(context);
    public INotificationRepository NotificationRepository => notificationRepository ??= new NotificationRepository(context);

    public async Task<bool> Complete()
    {
        try
        {
            return await context.SaveChangesAsync() > 0;
        }
        catch (DbUpdateException ex)
        {
            throw new Exception("An error occurred while saving changes", ex);
        }
    }

    public bool HasChanges()
    {
        return context.ChangeTracker.HasChanges();
    }
}
