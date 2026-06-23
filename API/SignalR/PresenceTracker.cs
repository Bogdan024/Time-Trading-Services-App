using System.Collections.Concurrent;

namespace API.SignalR;

public class PresenceTracker
{
    private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> OnlineUsers = new();

    public Task UserConnected(string memberId, string connectionId)
    {
        var connections = OnlineUsers.GetOrAdd(memberId, _ => new ConcurrentDictionary<string, byte>());
        connections.TryAdd(connectionId, 0);

        return Task.CompletedTask;
    }

    public Task UserDisconnected(string memberId, string connectionId)
    {
        if (OnlineUsers.TryGetValue(memberId, out var connections))
        {
            connections.TryRemove(connectionId, out _);

            if (connections.IsEmpty)
            {
                OnlineUsers.TryRemove(memberId, out _);
            }
        }

        return Task.CompletedTask;
    }

    public Task<string[]> GetOnlineUsers()
    {
        return Task.FromResult(OnlineUsers.Keys.OrderBy(x => x).ToArray());
    }

    public static Task<IReadOnlyList<string>> GetConnectionsForUser(string memberId)
    {
        if (OnlineUsers.TryGetValue(memberId, out var connections))
        {
            return Task.FromResult<IReadOnlyList<string>>(connections.Keys.ToList());
        }

        return Task.FromResult<IReadOnlyList<string>>([]);
    }
}
