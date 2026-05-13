using API.Entities;

namespace API.DTOs;

public class TimeTaskDto
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public int EstimatedHours { get; set; }
    public TaskLocationMode LocationMode { get; set; }
    public string? City { get; set; }
    public string? CountryCode { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public DateTime? DueAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public TimeTaskStatus Status { get; set; }
    public required ServiceCategoryDto ServiceCategory { get; set; }
    public required TaskMemberDto PostedByMember { get; set; }
    public TaskMemberDto? AcceptedByMember { get; set; }
}
