using API.Entities;
using API.Helpers;
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

    public async Task<Conversation> GetOrCreateGroupConversationAsync(CommunityGroup group)
    {
        var conversation = await context.Conversations
            .Include(x => x.Participants)
            .SingleOrDefaultAsync(x => x.Type == ConversationType.Group && x.GroupId == group.Id);

        if (conversation is not null) return conversation;

        conversation = new Conversation
        {
            Type = ConversationType.Group,
            Group = group,
            Title = group.Name,
            Participants = group.Members
                .Select(member => new ConversationParticipant { MemberId = member.MemberId })
                .ToList()
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

    public async Task<PaginatedResult<Conversation>> GetConversationsForMemberAsync(string memberId, ConversationParams conversationParams)
    {
        var query = ConversationQuery(memberId);

        if (conversationParams.Type.HasValue)
        {
            query = query.Where(x => x.Type == conversationParams.Type.Value);
        }

        query = query.OrderByDescending(x => x.Messages
            .Where(message => message.DeletedForMembers.All(deletion => deletion.MemberId != memberId))
            .Select(message => (DateTime?)message.CreatedAtUtc)
            .Max() ?? x.CreatedAtUtc);

        return await PaginationHelper.CreateAsync(query, conversationParams.PageNumber, conversationParams.PageSize);
    }

    public async Task<PaginatedResult<Message>> GetMessagesForConversationAsync(int conversationId, string memberId, MessageParams messageParams)
    {
        var isParticipant = await context.ConversationParticipants
            .AnyAsync(x => x.ConversationId == conversationId && x.MemberId == memberId);

        if (!isParticipant)
        {
            return EmptyMessagesResult(messageParams);
        }

        await context.ConversationParticipants
            .Where(x => x.ConversationId == conversationId && x.MemberId == memberId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.LastReadAtUtc, DateTime.UtcNow));

        var query = context.Messages
            .Include(x => x.SenderMember)
            .Include(x => x.DeletedForMembers)
            .Where(x => x.ConversationId == conversationId && x.DeletedForMembers.All(deletion => deletion.MemberId != memberId))
            .OrderByDescending(x => x.CreatedAtUtc);

        var messages = await PaginationHelper.CreateAsync(query, messageParams.PageNumber, messageParams.PageSize);
        messages.Items = messages.Items.OrderBy(x => x.CreatedAtUtc).ToList();

        return messages;
    }

    public async Task MarkConversationReadAsync(int conversationId, string memberId)
    {
        await context.ConversationParticipants
            .Where(x => x.ConversationId == conversationId && x.MemberId == memberId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.LastReadAtUtc, DateTime.UtcNow));
    }

    public async Task<Message?> GetMessageForMemberAsync(string messageId, string memberId)
    {
        return await context.Messages
            .Include(x => x.SenderMember)
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
            .AsSplitQuery()
            .Include(x => x.TimeTask)
            .Include(x => x.Group)
            .Include(x => x.Participants)
                .ThenInclude(x => x.Member)
            .Include(x => x.Messages)
                .ThenInclude(x => x.SenderMember)
            .Include(x => x.Messages)
                .ThenInclude(x => x.DeletedForMembers)
            .Where(x => x.Participants.Any(participant => participant.MemberId == memberId));
    }

    private static PaginatedResult<Message> EmptyMessagesResult(MessageParams messageParams)
    {
        return new PaginatedResult<Message>
        {
            Metadata = new PaginationMetadata
            {
                CurrentPage = messageParams.PageNumber,
                PageSize = messageParams.PageSize,
                TotalCount = 0,
                TotalPages = 0
            },
            Items = []
        };
    }
}


