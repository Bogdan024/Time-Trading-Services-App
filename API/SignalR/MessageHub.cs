using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR;

[Authorize]
public class MessageHub(
    IMessageRepository messageRepository,
    ConversationPresenceTracker conversationPresenceTracker,
    IHubContext<PresenceHub> presenceHub) : Hub
{
    private const string ConversationIdKey = "conversationId";

    public override async Task OnConnectedAsync()
    {
        var conversationId = GetConversationId();
        var memberId = GetMemberId();
        var conversation = await messageRepository.GetConversationForMemberAsync(conversationId, memberId);

        if (conversation is null)
        {
            throw new HubException("Conversation not found");
        }

        Context.Items[ConversationIdKey] = conversationId;
        await Groups.AddToGroupAsync(Context.ConnectionId, GetSignalRGroupName(conversationId));
        await conversationPresenceTracker.UserJoinedConversation(conversationId, memberId, Context.ConnectionId);
        await messageRepository.MarkConversationReadAsync(conversationId, memberId);
    }

    public async Task SendMessage(CreateMessageDto createMessageDto)
    {
        var conversationId = GetStoredConversationId();
        var memberId = GetMemberId();
        var conversation = await messageRepository.GetConversationForMemberAsync(conversationId, memberId);

        if (conversation is null)
        {
            throw new HubException("Conversation not found");
        }

        if (!conversation.CanSendMessages())
        {
            throw new HubException("This conversation is closed");
        }

        var content = createMessageDto.Content.Trim();

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new HubException("Message cannot be empty");
        }

        var message = new Message
        {
            ConversationId = conversation.Id,
            SenderMemberId = memberId,
            Content = content
        };

        messageRepository.AddMessage(message);

        if (!await messageRepository.SaveAllAsync())
        {
            throw new HubException("Failed to send message");
        }

        var createdMessage = await messageRepository.GetMessageForMemberAsync(message.Id, memberId);

        if (createdMessage is null)
        {
            throw new HubException("Failed to load message");
        }

        var activeMembers = await conversationPresenceTracker.GetActiveUsersForConversation(conversation.Id);

        foreach (var activeMemberId in activeMembers.Where(x => x != memberId))
        {
            await messageRepository.MarkConversationReadAsync(conversation.Id, activeMemberId);
        }

        await Clients.Group(GetSignalRGroupName(conversation.Id)).SendAsync("NewMessage", createdMessage.ToDto(memberId));
        await NotifyOnlineMembersOutsideConversation(conversation, createdMessage, activeMembers, memberId);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (Context.Items.TryGetValue(ConversationIdKey, out var conversationIdValue)
            && conversationIdValue is int conversationId)
        {
            await conversationPresenceTracker.UserLeftConversation(conversationId, GetMemberId(), Context.ConnectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    private async Task NotifyOnlineMembersOutsideConversation(
        Conversation conversation,
        Message createdMessage,
        IReadOnlyList<string> activeMembers,
        string senderMemberId)
    {
        foreach (var participant in conversation.Participants.Where(x => x.MemberId != senderMemberId && !activeMembers.Contains(x.MemberId)))
        {
            var connections = await PresenceTracker.GetConnectionsForUser(participant.MemberId);

            if (connections.Count > 0)
            {
                await presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived", createdMessage.ToDto(participant.MemberId));
            }
        }
    }

    private int GetConversationId()
    {
        var httpContext = Context.GetHttpContext();
        var conversationIdValue = httpContext?.Request.Query["conversationId"].ToString();

        if (!int.TryParse(conversationIdValue, out var conversationId))
        {
            throw new HubException("Conversation id is required");
        }

        return conversationId;
    }

    private int GetStoredConversationId()
    {
        if (Context.Items.TryGetValue(ConversationIdKey, out var conversationIdValue)
            && conversationIdValue is int conversationId)
        {
            return conversationId;
        }

        throw new HubException("Conversation id is missing");
    }

    private string GetMemberId()
    {
        return Context.User?.GetMemberId() ?? throw new HubException("Cannot get member id");
    }

    private static string GetSignalRGroupName(int conversationId)
    {
        return $"conversation-{conversationId}";
    }
}
