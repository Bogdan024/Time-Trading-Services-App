using API.DTOs;
using API.Entities;

namespace API.Extensions;

public static class MessageExtensions
{
    public static MessageDto ToDto(this Message message, string currentMemberId)
    {
        return new MessageDto
        {
            Id = message.Id,
            ConversationId = message.ConversationId,
            Content = message.Content,
            CreatedAtUtc = message.CreatedAtUtc,
            Sender = message.SenderMember.ToTaskMemberDto(),
            CurrentUserSender = message.SenderMemberId == currentMemberId
        };
    }

    public static ConversationDto ToDto(this Conversation conversation, string currentMemberId)
    {
        var visibleMessages = conversation.Messages
            .Where(message => message.DeletedForMembers.All(deletion => deletion.MemberId != currentMemberId))
            .OrderBy(message => message.CreatedAtUtc)
            .ToList();
        var latestMessage = visibleMessages.LastOrDefault();
        var currentParticipant = conversation.Participants.FirstOrDefault(x => x.MemberId == currentMemberId);
        var otherParticipant = conversation.Participants.FirstOrDefault(x => x.MemberId != currentMemberId);

        return new ConversationDto
        {
            Id = conversation.Id,
            Type = conversation.Type,
            TimeTaskId = conversation.TimeTaskId,
            TaskTitle = conversation.TimeTask?.Title,
            Title = conversation.Title,
            CreatedAtUtc = conversation.CreatedAtUtc,
            ClosedAtUtc = conversation.ClosedAtUtc,
            CanSendMessages = conversation.ClosedAtUtc is null && (conversation.Type != ConversationType.TaskDirect || conversation.TimeTask?.Status == TimeTaskStatus.InProgress),
            LatestMessage = latestMessage?.Content,
            LatestMessageAtUtc = latestMessage?.CreatedAtUtc,
            UnreadCount = visibleMessages.Count(message =>
                message.SenderMemberId != currentMemberId &&
                (currentParticipant?.LastReadAtUtc is null || message.CreatedAtUtc > currentParticipant.LastReadAtUtc)),
            OtherMember = otherParticipant?.Member.ToTaskMemberDto()
        };
    }
}

