using API.DTOs;
using API.Entities;

namespace API.Extensions;

public static class MemberExtensions
{
    public static TaskMemberDto ToTaskMemberDto(this Member member)
    {
        return new TaskMemberDto
        {
            Id = member.Id,
            DisplayName = member.DisplayName,
            AvatarUrl = member.AvatarUrl,
            City = member.City,
            CountryCode = member.CountryCode
        };
    }
}
