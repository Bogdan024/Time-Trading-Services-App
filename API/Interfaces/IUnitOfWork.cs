namespace API.Interfaces;

public interface IUnitOfWork
{
    IMemberRepository MemberRepository { get; }
    ITimeTaskRepository TimeTaskRepository { get; }
    IReviewRepository ReviewRepository { get; }
    IMessageRepository MessageRepository { get; }
    IGroupRepository GroupRepository { get; }
    IModerationRepository ModerationRepository { get; }
    INotificationRepository NotificationRepository { get; }
    Task<bool> Complete();
    bool HasChanges();
}
