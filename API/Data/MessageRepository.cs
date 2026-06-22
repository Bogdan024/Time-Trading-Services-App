using API.Entities;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class MessageRepository(AppDbContext context) : IMessageRepository
{
    public void AddMessage(Message message)
    {
        context.Messages.Add(message);
    }

    public async Task<Conversation> GetOrCreateTaskConversationAsync(TimeTask task)
    {
        var conversation = await context.Conversations
            .Include(x => x.Participants)
            .SingleOrDefaultAsync(x => x.Type == ConversationType.TaskDirect && x.TimeTaskId == task.Id);

        if (conversation is not null) return conversation;

        if (task.AcceptedByMemberId is null)
        {
            throw new InvalidOperationException("Task must be accepted before a conversation can be created");
        }

        conversation = new Conversation
        {
            Type = ConversationType.TaskDirect,
            TimeTaskId = task.Id,
            Title = task.Title,
            Participants =
            [
                new ConversationParticipant { MemberId = task.PostedByMemberId },
                new ConversationParticipant { MemberId = task.AcceptedByMemberId }
            ]
        };

        context.Conversations.Add(conversation);

        return conversation;
    }

    public async Task CloseTaskConversationAsync(int taskId)
    {
        var conversation = await context.Conversations
            .SingleOrDefaultAsync(x => x.Type == ConversationType.TaskDirect && x.TimeTaskId == taskId);

        if (conversation is not null && conversation.ClosedAtUtc is null)
        {
            conversation.ClosedAtUtc = DateTime.UtcNow;
        }
    }

    public async Task<Conversation?> GetConversationForTaskAsync(int taskId, string memberId)
    {
        return await ConversationQuery(memberId)
            .SingleOrDefaultAsync(x => x.Type == ConversationType.TaskDirect && x.TimeTaskId == taskId);
    }

    public async Task<Conversation?> GetConversationForMemberAsync(int conversationId, string memberId)
    {
        return await ConversationQuery(memberId)
            .SingleOrDefaultAsync(x => x.Id == conversationId);
    }

    public async Task<IReadOnlyList<Conversation>> GetConversationsForMemberAsync(string memberId)
    {
        var conversations = await ConversationQuery(memberId)
            .ToListAsync();

        return conversations
            .OrderByDescending(x => x.Messages
                .Where(message => message.DeletedForMembers.All(deletion => deletion.MemberId != memberId))
                .Select(message => (DateTime?)message.CreatedAtUtc)
                .DefaultIfEmpty(x.CreatedAtUtc)
                .Max())
            .ToList();
    }

    public async Task<IReadOnlyList<Message>> GetMessagesForConversationAsync(int conversationId, string memberId)
    {
        var isParticipant = await context.ConversationParticipants
            .AnyAsync(x => x.ConversationId == conversationId && x.MemberId == memberId);

        if (!isParticipant) return [];

        await context.ConversationParticipants
            .Where(x => x.ConversationId == conversationId && x.MemberId == memberId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.LastReadAtUtc, DateTime.UtcNow));

        return await context.Messages
            .Include(x => x.SenderMember)
            .Include(x => x.DeletedForMembers)
            .Where(x => x.ConversationId == conversationId && x.DeletedForMembers.All(deletion => deletion.MemberId != memberId))
            .OrderBy(x => x.CreatedAtUtc)
            .ToListAsync();
    }

    public async Task<Message?> GetMessageForMemberAsync(string messageId, string memberId)
    {
        return await context.Messages
            .Include(x => x.Conversation)
                .ThenInclude(x => x.Participants)
            .Include(x => x.DeletedForMembers)
            .SingleOrDefaultAsync(x => x.Id == messageId && x.Conversation.Participants.Any(p => p.MemberId == memberId) && x.DeletedForMembers.All(deletion => deletion.MemberId != memberId));
    }

    public void DeleteMessageForMember(Message message, string memberId)
    {
        if (message.DeletedForMembers.Any(x => x.MemberId == memberId)) return;

        message.DeletedForMembers.Add(new MessageDeletion
        {
            MessageId = message.Id,
            MemberId = memberId
        });
    }

    public async Task<bool> SaveAllAsync()
    {
        return await context.SaveChangesAsync() > 0;
    }

    private IQueryable<Conversation> ConversationQuery(string memberId)
    {
        return context.Conversations
            .Include(x => x.TimeTask)
            .Include(x => x.Participants)
                .ThenInclude(x => x.Member)
            .Include(x => x.Messages)
                .ThenInclude(x => x.SenderMember)
            .Include(x => x.Messages)
                .ThenInclude(x => x.DeletedForMembers)
            .Where(x => x.Participants.Any(participant => participant.MemberId == memberId));
    }
}

