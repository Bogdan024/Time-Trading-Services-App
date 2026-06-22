using System.Text.Json.Serialization;

namespace API.Entities;

public class ConversationParticipant
{
    public int ConversationId { get; set; }

    [JsonIgnore]
    public Conversation Conversation { get; set; } = null!;

    public required string MemberId { get; set; }

    [JsonIgnore]
    public Member Member { get; set; } = null!;

    public DateTime JoinedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? LastReadAtUtc { get; set; }
}
