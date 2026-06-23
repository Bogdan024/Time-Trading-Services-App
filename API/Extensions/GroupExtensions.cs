using API.DTOs;
using API.Entities;

namespace API.Extensions;

public static class GroupExtensions
{
    public static GroupDto ToDto(this CommunityGroup group, string currentMemberId)
    {
        return new GroupDto
        {
            Id = group.Id,
            Name = group.Name,
            Description = group.Description,
            Theme = group.Theme,
            City = group.City,
            CountryCode = group.CountryCode,
            ModerationStatus = group.ModerationStatus.ToString(),
            RejectionReason = group.RejectionReason,
            CreatedAtUtc = group.CreatedAtUtc,
            MemberCount = group.Members.Count,
            IsMember = group.Members.Any(x => x.MemberId == currentMemberId),
            IsOwner = group.OwnerMemberId == currentMemberId,
            ConversationId = group.Conversation?.Id,
            Owner = group.OwnerMember.ToTaskMemberDto()
        };
    }
}
