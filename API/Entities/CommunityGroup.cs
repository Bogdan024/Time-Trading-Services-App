using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API.Entities;

public class CommunityGroup
{
    public int Id { get; set; }

    [MaxLength(80)]
    public required string Name { get; set; }

    [MaxLength(1000)]
    public required string Description { get; set; }

    [MaxLength(60)]
    public string? Theme { get; set; }

    [MaxLength(80)]
    public string? City { get; set; }

    [MaxLength(2)]
    public string? CountryCode { get; set; }

    public ModerationStatus ModerationStatus { get; set; } = ModerationStatus.PendingApproval;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public string OwnerMemberId { get; set; } = null!;
    public string? ReviewedByMemberId { get; set; }
    public DateTime? ReviewedAtUtc { get; set; }

    [MaxLength(1000)]
    public string? RejectionReason { get; set; }

    [JsonIgnore]
    public Member OwnerMember { get; set; } = null!;

    [JsonIgnore]
    public Member? ReviewedByMember { get; set; }

    [JsonIgnore]
    public List<CommunityGroupMember> Members { get; set; } = [];

    [JsonIgnore]
    public Conversation? Conversation { get; set; }
}
