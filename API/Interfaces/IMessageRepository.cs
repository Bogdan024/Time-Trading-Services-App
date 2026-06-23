using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces;

public interface IMessageRepository
{
    void AddMessage(Message message);
    Task<bool> SaveAllAsync();
    Task<Conversation> GetOrCreateTaskConversationAsync(TimeTask task);
    Task<Conversation> GetOrCreateGroupConversationAsync(CommunityGroup group);
    Task CloseTaskConversationAsync(int taskId);
    Task<Conversation?> GetConversationForTaskAsync(int taskId, string memberId);
    Task<Conversation?> GetConversationForMemberAsync(int conversationId, string memberId);
    Task<PaginatedResult<Conversation>> GetConversationsForMemberAsync(string memberId, ConversationParams conversationParams);
    Task<PaginatedResult<Message>> GetMessagesForConversationAsync(int conversationId, string memberId, MessageParams messageParams);
    Task<Message?> GetMessageForMemberAsync(string messageId, string memberId);
    Task MarkConversationReadAsync(int conversationId, string memberId);
    void DeleteMessageForMember(Message message, string memberId);
}
