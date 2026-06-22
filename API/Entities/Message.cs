using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API.Entities;

public class Message
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [MaxLength(2000)]
    public required string Content { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public int ConversationId { get; set; }

    [JsonIgnore]
    public Conversation Conversation { get; set; } = null!;

    public required string SenderMemberId { get; set; }

    [JsonIgnore]
    public Member SenderMember { get; set; } = null!;

    [JsonIgnore]
    public List<MessageDeletion> DeletedForMembers { get; set; } = [];
}
