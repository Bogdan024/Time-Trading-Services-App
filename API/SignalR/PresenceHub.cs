using API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR;

[Authorize]
public class PresenceHub(PresenceTracker presenceTracker) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var memberId = GetMemberId();
        await presenceTracker.UserConnected(memberId, Context.ConnectionId);
        await Clients.Others.SendAsync("UserOnline", memberId);

        var onlineUsers = await presenceTracker.GetOnlineUsers();
        await Clients.All.SendAsync("GetOnlineUsers", onlineUsers);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var memberId = GetMemberId();
        await presenceTracker.UserDisconnected(memberId, Context.ConnectionId);
        await Clients.Others.SendAsync("UserOffline", memberId);

        var onlineUsers = await presenceTracker.GetOnlineUsers();
        await Clients.All.SendAsync("GetOnlineUsers", onlineUsers);

        await base.OnDisconnectedAsync(exception);
    }

    private string GetMemberId()
    {
        return Context.User?.GetMemberId() ?? throw new HubException("Cannot get member id");
    }
}
