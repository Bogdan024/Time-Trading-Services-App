namespace API.DTOs;

public class ModerationReportDto
{
    public int Id { get; set; }
    public required string TargetType { get; set; }
    public int? TargetIntId { get; set; }
    public string? TargetStringId { get; set; }
    public required string Reason { get; set; }
    public string? Details { get; set; }
    public required string Status { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ReviewedAtUtc { get; set; }
    public string? ModeratorNotes { get; set; }
    public TaskMemberDto? Reporter { get; set; }
    public TaskMemberDto? ReviewedBy { get; set; }
    public string? TargetTitle { get; set; }
    public string? TargetSummary { get; set; }
}
