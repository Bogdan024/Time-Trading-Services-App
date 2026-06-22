namespace API.DTOs;

public class GroupDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public string? Theme { get; set; }
    public string? City { get; set; }
    public string? CountryCode { get; set; }
    public bool IsPublic { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public int MemberCount { get; set; }
    public bool IsMember { get; set; }
    public bool IsOwner { get; set; }
    public int? ConversationId { get; set; }
    public TaskMemberDto? Owner { get; set; }
}
