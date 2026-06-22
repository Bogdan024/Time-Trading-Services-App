using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class MessagesController(IMessageRepository messageRepository) : BaseApiController
{
    [HttpGet("conversations")]
    public async Task<ActionResult<IReadOnlyList<ConversationDto>>> GetConversations()
    {
        var memberId = User.GetMemberId();
        var conversations = await messageRepository.GetConversationsForMemberAsync(memberId);

        return Ok(conversations.Select(x => x.ToDto(memberId)));
    }

    [HttpGet("conversations/{conversationId:int}")]
    public async Task<ActionResult<IReadOnlyList<MessageDto>>> GetConversationThread(int conversationId)
    {
        var memberId = User.GetMemberId();
        var conversation = await messageRepository.GetConversationForMemberAsync(conversationId, memberId);

        if (conversation is null) return NotFound();

        var messages = await messageRepository.GetMessagesForConversationAsync(conversationId, memberId);

        return Ok(messages.Select(x => x.ToDto(memberId)));
    }

    [HttpGet("tasks/{taskId:int}")]
    public async Task<ActionResult<ConversationDto>> GetTaskConversation(int taskId)
    {
        var memberId = User.GetMemberId();
        var conversation = await messageRepository.GetConversationForTaskAsync(taskId, memberId);

        if (conversation is null) return NotFound();

        return Ok(conversation.ToDto(memberId));
    }

    [HttpGet("tasks/{taskId:int}/thread")]
    public async Task<ActionResult<IReadOnlyList<MessageDto>>> GetTaskConversationThread(int taskId)
    {
        var memberId = User.GetMemberId();
        var conversation = await messageRepository.GetConversationForTaskAsync(taskId, memberId);

        if (conversation is null) return NotFound();

        var messages = await messageRepository.GetMessagesForConversationAsync(conversation.Id, memberId);

        return Ok(messages.Select(x => x.ToDto(memberId)));
    }

    [HttpPost("conversations/{conversationId:int}")]
    public async Task<ActionResult<MessageDto>> SendConversationMessage(int conversationId, CreateMessageDto createMessageDto)
    {
        var memberId = User.GetMemberId();
        var conversation = await messageRepository.GetConversationForMemberAsync(conversationId, memberId);

        if (conversation is null) return NotFound();
        if (!CanSendMessage(conversation)) return BadRequest("This conversation is closed");

        return await SendMessage(conversation, memberId, createMessageDto);
    }

    [HttpPost("tasks/{taskId:int}")]
    public async Task<ActionResult<MessageDto>> SendTaskMessage(int taskId, CreateMessageDto createMessageDto)
    {
        var memberId = User.GetMemberId();
        var conversation = await messageRepository.GetConversationForTaskAsync(taskId, memberId);

        if (conversation is null) return NotFound();
        if (!CanSendMessage(conversation)) return BadRequest("This conversation is closed");

        return await SendMessage(conversation, memberId, createMessageDto);
    }

    [HttpDelete("{messageId}")]
    public async Task<ActionResult> DeleteMessage(string messageId)
    {
        var memberId = User.GetMemberId();
        var message = await messageRepository.GetMessageForMemberAsync(messageId, memberId);

        if (message is null) return NotFound();

        messageRepository.DeleteMessageForMember(message, memberId);

        if (await messageRepository.SaveAllAsync()) return NoContent();

        return BadRequest("Failed to delete message");
    }

    private async Task<ActionResult<MessageDto>> SendMessage(Conversation conversation, string memberId, CreateMessageDto createMessageDto)
    {
        var content = createMessageDto.Content.Trim();

        if (string.IsNullOrWhiteSpace(content)) return BadRequest("Message cannot be empty");

        var message = new Message
        {
            ConversationId = conversation.Id,
            SenderMemberId = memberId,
            Content = content
        };

        messageRepository.AddMessage(message);

        if (!await messageRepository.SaveAllAsync()) return BadRequest("Failed to send message");

        var messages = await messageRepository.GetMessagesForConversationAsync(conversation.Id, memberId);
        var createdMessage = messages.SingleOrDefault(x => x.Id == message.Id);

        if (createdMessage is null) return BadRequest("Failed to load created message");

        return Ok(createdMessage.ToDto(memberId));
    }

    private static bool CanSendMessage(Conversation conversation)
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
