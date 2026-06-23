using API.Entities;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class GroupRepository(AppDbContext context) : IGroupRepository
{
    public void AddGroup(CommunityGroup group)
    {
        context.CommunityGroups.Add(group);
    }

    public async Task<IReadOnlyList<CommunityGroup>> GetGroupsAsync(string currentMemberId)
    {
        return await GroupQuery(currentMemberId)
            .OrderByDescending(x => x.Members.Any(member => member.MemberId == currentMemberId))
            .ThenByDescending(x => x.CreatedAtUtc)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<CommunityGroup>> GetPendingGroupsAsync()
    {
        return await context.CommunityGroups
            .Include(x => x.OwnerMember)
            .Include(x => x.Members)
            .Where(x => x.ModerationStatus == ModerationStatus.PendingApproval)
            .OrderBy(x => x.CreatedAtUtc)
            .ToListAsync();
    }

    public async Task<CommunityGroup?> GetGroupByIdAsync(int id, string currentMemberId)
    {
        return await GroupQuery(currentMemberId).SingleOrDefaultAsync(x => x.Id == id);
    }

    public async Task<CommunityGroup?> GetGroupForModerationAsync(int id)
    {
        return await context.CommunityGroups
            .Include(x => x.OwnerMember)
            .Include(x => x.Members)
            .SingleOrDefaultAsync(x => x.Id == id);
    }

    public async Task<bool> GroupNameExistsAsync(string name)
    {
        var normalizedName = name.Trim().ToLower();

        return await context.CommunityGroups.AnyAsync(x => x.Name.ToLower() == normalizedName);
    }

    public async Task<bool> IsGroupMemberAsync(int groupId, string memberId)
    {
        return await context.CommunityGroupMembers.AnyAsync(x => x.CommunityGroupId == groupId && x.MemberId == memberId);
    }

    public void JoinGroup(CommunityGroup group, string memberId)
    {
        if (group.Members.Any(x => x.MemberId == memberId)) return;

        group.Members.Add(new CommunityGroupMember
        {
            CommunityGroupId = group.Id,
            MemberId = memberId
        });

        if (group.Conversation is not null && group.Conversation.Participants.All(x => x.MemberId != memberId))
        {
            group.Conversation.Participants.Add(new ConversationParticipant
            {
                ConversationId = group.Conversation.Id,
                MemberId = memberId
            });
        }
    }

    public void LeaveGroup(CommunityGroup group, string memberId)
    {
        var membership = group.Members.SingleOrDefault(x => x.MemberId == memberId);

        if (membership is not null)
        {
            context.CommunityGroupMembers.Remove(membership);
        }

        var participant = group.Conversation?.Participants.SingleOrDefault(x => x.MemberId == memberId);

        if (participant is not null)
        {
            context.ConversationParticipants.Remove(participant);
        }
    }

    public async Task<bool> SaveAllAsync()
    {
        return await context.SaveChangesAsync() > 0;
    }

    private IQueryable<CommunityGroup> GroupQuery(string currentMemberId)
    {
        return context.CommunityGroups
            .Include(x => x.OwnerMember)
            .Include(x => x.Members)
            .Include(x => x.Conversation)
                .ThenInclude(x => x!.Participants)
            .Where(x => x.ModerationStatus == ModerationStatus.Approved || x.OwnerMemberId == currentMemberId);
    }
}


