using API.Entities;

namespace API.DTOs;

public class NotificationDto
{
    public int Id { get; set; }
    public NotificationType Type { get; set; }
    public required string Title { get; set; }
    public required string Body { get; set; }
    public int? TimeTaskId { get; set; }
    public int? GroupId { get; set; }
    public int? ConversationId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ReadAtUtc { get; set; }
    public bool IsRead => ReadAtUtc.HasValue;
}
