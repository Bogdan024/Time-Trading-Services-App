using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API.Entities;

public class Member
{
    public string Id { get; set; } = null!;
    public required string DisplayName { get; set; }
    public string? About { get; set; }
    public string? AvatarUrl { get; set; }
    public string? City { get; set; }
    public string? CountryCode { get; set; }
    public bool IsProfilePublic { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime LastActiveAtUtc { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public List<Photo> Photos { get; set; } = [];
    public List<MemberSkill> OfferedSkills { get; set; } = [];
    public List<MemberNeed> NeedsHelpWith { get; set; } = [];
    public List<MemberAvailabilitySlot> AvailabilitySlots { get; set; } = [];

    [JsonIgnore]
    [ForeignKey(nameof(Id))]
    public AppUser User { get; set; } = null!;
}
