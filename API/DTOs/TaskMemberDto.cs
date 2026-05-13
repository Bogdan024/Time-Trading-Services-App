namespace API.DTOs;

public class TaskMemberDto
{
    public required string Id { get; set; }
    public required string DisplayName { get; set; }
    public string? AvatarUrl { get; set; }
    public string? City { get; set; }
    public string? CountryCode { get; set; }
}
