using System.Text.Json.Serialization;

namespace API.Entities;

public class Conversation
{
    public int Id { get; set; }
    public ConversationType Type { get; set; }
    public int? TimeTaskId { get; set; }
    public int? GroupId { get; set; }
    public string? Title { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAtUtc { get; set; }

    [JsonIgnore]
    public TimeTask? TimeTask { get; set; }

    [JsonIgnore]
    public CommunityGroup? Group { get; set; }

    [JsonIgnore]
    public List<ConversationParticipant> Participants { get; set; } = [];

    [JsonIgnore]
    public List<Message> Messages { get; set; } = [];
}


