using API.Entities;

namespace API.DTOs;

public class ConversationDto
{
    public int Id { get; set; }
    public ConversationType Type { get; set; }
    public int? TimeTaskId { get; set; }
    public int? GroupId { get; set; }
    public string? TaskTitle { get; set; }
    public string? Title { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ClosedAtUtc { get; set; }
    public bool CanSendMessages { get; set; }
    public int UnreadCount { get; set; }
    public string? LatestMessage { get; set; }
    public DateTime? LatestMessageAtUtc { get; set; }
    public TaskMemberDto? OtherMember { get; set; }
}


