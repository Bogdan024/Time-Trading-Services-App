using System.Text.Json.Serialization;

namespace API.Entities;

public class CommunityGroupMember
{
    public int CommunityGroupId { get; set; }
    public string MemberId { get; set; } = null!;
    public GroupMemberRole Role { get; set; } = GroupMemberRole.Member;
    public DateTime JoinedAtUtc { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public CommunityGroup CommunityGroup { get; set; } = null!;

    [JsonIgnore]
    public Member Member { get; set; } = null!;
}
