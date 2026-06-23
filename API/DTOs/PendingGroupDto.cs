namespace API.DTOs;

public class PendingGroupDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public string? Theme { get; set; }
    public string? City { get; set; }
    public string? CountryCode { get; set; }
    public required string ModerationStatus { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public TaskMemberDto? Owner { get; set; }
}
