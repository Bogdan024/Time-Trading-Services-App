namespace API.DTOs;

public class TimeTransactionDto
{
    public int Id { get; set; }
    public int TimeTaskId { get; set; }
    public required string TaskTitle { get; set; }
    public int Hours { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string? Note { get; set; }
    public required TaskMemberDto FromMember { get; set; }
    public required TaskMemberDto ToMember { get; set; }
}
