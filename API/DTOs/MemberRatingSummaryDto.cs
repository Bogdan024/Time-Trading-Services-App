namespace API.DTOs;

public class MemberRatingSummaryDto
{
    public required string MemberId { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
}
