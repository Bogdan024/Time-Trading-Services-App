using API.Entities;

namespace API.Extensions;

public static class ConversationExtensions
{
    public static bool CanSendMessages(this Conversation conversation)
    {
        if (conversation.ClosedAtUtc is not null) return false;

        return conversation.Type switch
        {
            ConversationType.TaskDirect => conversation.TimeTask?.Status == TimeTaskStatus.InProgress,
            ConversationType.Group => true,
            _ => false
        };
    }
}
