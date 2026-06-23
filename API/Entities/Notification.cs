using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API.Entities;

public class Notification
{
    public int Id { get; set; }
    public required string RecipientMemberId { get; set; }

    [JsonIgnore]
    public Member RecipientMember { get; set; } = null!;

    public NotificationType Type { get; set; }

    [MaxLength(120)]
    public required string Title { get; set; }

    [MaxLength(300)]
    public required string Body { get; set; }

    public int? TimeTaskId { get; set; }
    public int? GroupId { get; set; }
    public int? ConversationId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAtUtc { get; set; }
}

public enum NotificationType
{
    TaskApplicationReceived = 1,
    TaskApplicationAccepted = 2,
    TaskCompleted = 3,
    TaskCancelled = 4
}
