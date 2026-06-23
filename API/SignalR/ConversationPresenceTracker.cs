using System.Collections.Concurrent;

namespace API.SignalR;

public class ConversationPresenceTracker
{
    private static readonly ConcurrentDictionary<int, ConcurrentDictionary<string, ConcurrentDictionary<string, byte>>> ConversationUsers = new();

    public Task UserJoinedConversation(int conversationId, string memberId, string connectionId)
    {
        var users = ConversationUsers.GetOrAdd(conversationId, _ => new ConcurrentDictionary<string, ConcurrentDictionary<string, byte>>());
        var connections = users.GetOrAdd(memberId, _ => new ConcurrentDictionary<string, byte>());
        connections.TryAdd(connectionId, 0);

        return Task.CompletedTask;
    }

    public Task UserLeftConversation(int conversationId, string memberId, string connectionId)
    {
        if (ConversationUsers.TryGetValue(conversationId, out var users)
            && users.TryGetValue(memberId, out var connections))
        {
            connections.TryRemove(connectionId, out _);

            if (connections.IsEmpty)
            {
                users.TryRemove(memberId, out _);
            }

            if (users.IsEmpty)
            {
                ConversationUsers.TryRemove(conversationId, out _);
            }
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<string>> GetActiveUsersForConversation(int conversationId)
    {
        if (ConversationUsers.TryGetValue(conversationId, out var users))
        {
            return Task.FromResult<IReadOnlyList<string>>(users.Keys.ToList());
        }

        return Task.FromResult<IReadOnlyList<string>>([]);
    }
}
