using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API.Entities;

public class MemberReview
{
    public int Id { get; set; }

    public int TimeTaskId { get; set; }

    [JsonIgnore]
    public TimeTask TimeTask { get; set; } = null!;

    public required string ReviewerMemberId { get; set; }

    [JsonIgnore]
    public Member ReviewerMember { get; set; } = null!;

    public required string ReviewedMemberId { get; set; }

    [JsonIgnore]
    public Member ReviewedMember { get; set; } = null!;

    [Range(1, 5)]
    public int Rating { get; set; }

    [MaxLength(500)]
    public string? Comment { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
