using API.DTOs;
using API.Entities;

namespace API.Interfaces;

public interface IMessageRepository
{
    void AddMessage(Message message);
    Task<bool> SaveAllAsync();
    Task<Conversation> GetOrCreateTaskConversationAsync(TimeTask task);
    Task CloseTaskConversationAsync(int taskId);
    Task<Conversation?> GetConversationForTaskAsync(int taskId, string memberId);
    Task<Conversation?> GetConversationForMemberAsync(int conversationId, string memberId);
    Task<IReadOnlyList<Conversation>> GetConversationsForMemberAsync(string memberId);
    Task<IReadOnlyList<Message>> GetMessagesForConversationAsync(int conversationId, string memberId);
    Task<Message?> GetMessageForMemberAsync(string messageId, string memberId);
    void DeleteMessageForMember(Message message, string memberId);
}
