using API.Entities;

namespace API.DTOs;

public class TaskApplicationDto
{
    public int Id { get; set; }
    public int TimeTaskId { get; set; }
    public TaskApplicationStatus Status { get; set; }
    public string? Message { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public bool MatchesTaskCategory { get; set; }
    public double? ApplicantAverageRating { get; set; }
    public int ApplicantReviewCount { get; set; }
    public required TaskMemberDto Applicant { get; set; }
}
