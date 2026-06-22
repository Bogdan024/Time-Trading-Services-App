namespace API.DTOs;

public class MessageDto
{
    public required string Id { get; set; }
    public int ConversationId { get; set; }
    public required string Content { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public required TaskMemberDto Sender { get; set; }
    public bool CurrentUserSender { get; set; }
}
