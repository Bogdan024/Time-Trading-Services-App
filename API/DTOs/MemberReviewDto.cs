namespace API.DTOs;

public class MemberReviewDto
{
    public int Id { get; set; }
    public int TimeTaskId { get; set; }
    public required string TaskTitle { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public required TaskMemberDto Reviewer { get; set; }
    public required TaskMemberDto ReviewedMember { get; set; }
}
