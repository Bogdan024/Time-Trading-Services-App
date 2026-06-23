using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class MessagesController(IUnitOfWork uow) : BaseApiController
{
    [HttpGet("conversations")]
    public async Task<ActionResult<PaginatedResult<ConversationDto>>> GetConversations([FromQuery] ConversationParams conversationParams)
    {
        var memberId = User.GetMemberId();
        var conversations = await uow.MessageRepository.GetConversationsForMemberAsync(memberId, conversationParams);

        return Ok(new PaginatedResult<ConversationDto>
        {
            Metadata = conversations.Metadata,
            Items = conversations.Items.Select(x => x.ToDto(memberId)).ToList()
        });
    }

    [HttpGet("conversations/{conversationId:int}")]
    public async Task<ActionResult<PaginatedResult<MessageDto>>> GetConversationThread(int conversationId, [FromQuery] MessageParams messageParams)
    {
        var memberId = User.GetMemberId();
        var conversation = await uow.MessageRepository.GetConversationForMemberAsync(conversationId, memberId);

        if (conversation is null) return NotFound();

        return await GetConversationMessages(conversation.Id, memberId, messageParams);
    }

    [HttpGet("tasks/{taskId:int}")]
    public async Task<ActionResult<ConversationDto>> GetTaskConversation(int taskId)
    {
        var memberId = User.GetMemberId();
        var conversation = await uow.MessageRepository.GetConversationForTaskAsync(taskId, memberId);

        if (conversation is null) return NotFound();

        return Ok(conversation.ToDto(memberId));
    }

    [HttpGet("tasks/{taskId:int}/thread")]
    public async Task<ActionResult<PaginatedResult<MessageDto>>> GetTaskConversationThread(int taskId, [FromQuery] MessageParams messageParams)
    {
        var memberId = User.GetMemberId();
        var conversation = await uow.MessageRepository.GetConversationForTaskAsync(taskId, memberId);

        if (conversation is null) return NotFound();

        return await GetConversationMessages(conversation.Id, memberId, messageParams);
    }

    [HttpPost("conversations/{conversationId:int}")]
    public async Task<ActionResult<MessageDto>> SendConversationMessage(int conversationId, CreateMessageDto createMessageDto)
    {
        var memberId = User.GetMemberId();
        var conversation = await uow.MessageRepository.GetConversationForMemberAsync(conversationId, memberId);

        return await SendMessage(conversation, memberId, createMessageDto);
    }

    [HttpPost("tasks/{taskId:int}")]
    public async Task<ActionResult<MessageDto>> SendTaskMessage(int taskId, CreateMessageDto createMessageDto)
    {
        var memberId = User.GetMemberId();
        var conversation = await uow.MessageRepository.GetConversationForTaskAsync(taskId, memberId);

        return await SendMessage(conversation, memberId, createMessageDto);
    }

    [HttpDelete("{messageId}")]
    public async Task<ActionResult> DeleteMessage(string messageId)
    {
        var memberId = User.GetMemberId();
        var message = await uow.MessageRepository.GetMessageForMemberAsync(messageId, memberId);

        if (message is null) return NotFound();

        uow.MessageRepository.DeleteMessageForMember(message, memberId);

        if (await uow.Complete()) return NoContent();

        return BadRequest("Failed to delete message");
    }

    private async Task<ActionResult<PaginatedResult<MessageDto>>> GetConversationMessages(int conversationId, string memberId, MessageParams messageParams)
    {
        var messages = await uow.MessageRepository.GetMessagesForConversationAsync(conversationId, memberId, messageParams);

        return Ok(new PaginatedResult<MessageDto>
        {
            Metadata = messages.Metadata,
            Items = messages.Items.Select(x => x.ToDto(memberId)).ToList()
        });
    }

    private async Task<ActionResult<MessageDto>> SendMessage(Conversation? conversation, string memberId, CreateMessageDto createMessageDto)
    {
        if (conversation is null) return NotFound();
        if (!conversation.CanSendMessages()) return BadRequest("This conversation is closed");

        var content = createMessageDto.Content.Trim();

        if (string.IsNullOrWhiteSpace(content)) return BadRequest("Message cannot be empty");

        var message = new Message
        {
            ConversationId = conversation.Id,
            SenderMemberId = memberId,
            Content = content
        };

        uow.MessageRepository.AddMessage(message);

        if (!await uow.Complete()) return BadRequest("Failed to send message");

        var createdMessage = await uow.MessageRepository.GetMessageForMemberAsync(message.Id, memberId);

        if (createdMessage is null) return BadRequest("Failed to load created message");

        return Ok(createdMessage.ToDto(memberId));
    }
}
