using System.Text.Json.Serialization;

namespace API.Entities;

public class MessageDeletion
{
    public required string MessageId { get; set; }

    [JsonIgnore]
    public Message Message { get; set; } = null!;

    public required string MemberId { get; set; }

    [JsonIgnore]
    public Member Member { get; set; } = null!;

    public DateTime DeletedAtUtc { get; set; } = DateTime.UtcNow;
}
