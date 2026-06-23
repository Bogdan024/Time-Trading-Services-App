using API.Entities;

namespace API.Interfaces;

public interface IGroupRepository
{
    void AddGroup(CommunityGroup group);
    Task<bool> SaveAllAsync();
    Task<IReadOnlyList<CommunityGroup>> GetGroupsAsync(string currentMemberId);
    Task<IReadOnlyList<CommunityGroup>> GetPendingGroupsAsync();
    Task<CommunityGroup?> GetGroupByIdAsync(int id, string currentMemberId);
    Task<CommunityGroup?> GetGroupForModerationAsync(int id);
    Task<bool> GroupNameExistsAsync(string name);
    Task<bool> IsGroupMemberAsync(int groupId, string memberId);
    void JoinGroup(CommunityGroup group, string memberId);
    void LeaveGroup(CommunityGroup group, string memberId);
}
