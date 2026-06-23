using API.Entities;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class MemberRepository(AppDbContext context) : IMemberRepository
{
    public async Task<Member?> GetMemberByIdAsync(string id)
    {
        return await context.Members
            .Include(x => x.Photos)
            .Include(x => x.OfferedSkills)
                .ThenInclude(x => x.ServiceCategory)
            .Include(x => x.NeedsHelpWith)
                .ThenInclude(x => x.ServiceCategory)
            .Include(x => x.AvailabilitySlots)
            .SingleOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Member?> GetMemberForUpdateAsync(string id)
    {
        return await context.Members
            .Include(x => x.User)
            .SingleOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Member?> GetMemberForAvatarUpdateAsync(string id)
    {
        return await context.Members
            .Include(x => x.User)
            .Include(x => x.Photos)
            .SingleOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Member?> GetMemberWithServicePreferencesForUpdateAsync(string id)
    {
        return await context.Members
            .Include(x => x.OfferedSkills)
            .Include(x => x.NeedsHelpWith)
            .SingleOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Member?> GetMemberWithAvailabilityForUpdateAsync(string id)
    {
        return await context.Members
            .Include(x => x.AvailabilitySlots)
            .SingleOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IReadOnlyList<Member>> GetMembersAsync()
    {
        return await context.Members
            .Where(x => x.IsProfilePublic && x.Id != "admin-user-id")
            .Include(x => x.Photos)
            .Include(x => x.OfferedSkills)
                .ThenInclude(x => x.ServiceCategory)
            .Include(x => x.NeedsHelpWith)
                .ThenInclude(x => x.ServiceCategory)
            .Include(x => x.AvailabilitySlots)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Photo>> GetPhotosForMemberAsync(string memberId)
    {
        return await context.Members
            .Where(x => x.Id == memberId)
            .SelectMany(x => x.Photos)
            .ToListAsync();
    }

    public async Task<MemberSkill?> GetMemberSkillForUpdateAsync(string memberId, int skillId)
    {
        return await context.MemberSkills
            .SingleOrDefaultAsync(x => x.Id == skillId && x.MemberId == memberId);
    }

    public async Task<MemberNeed?> GetMemberNeedForUpdateAsync(string memberId, int needId)
    {
        return await context.MemberNeeds
            .SingleOrDefaultAsync(x => x.Id == needId && x.MemberId == memberId);
    }

    public async Task<MemberAvailabilitySlot?> GetAvailabilitySlotForUpdateAsync(string memberId, int slotId)
    {
        return await context.MemberAvailabilitySlots
            .SingleOrDefaultAsync(x => x.Id == slotId && x.MemberId == memberId);
    }

    public async Task<bool> ServiceCategoryExistsAsync(int serviceCategoryId)
    {
        return await context.ServiceCategories.AnyAsync(x => x.Id == serviceCategoryId);
    }

    public async Task<bool> SaveAllAsync()
    {
        return await context.SaveChangesAsync() > 0;
    }

    public void DeleteMemberSkill(MemberSkill skill)
    {
        context.MemberSkills.Remove(skill);
    }

    public void DeleteMemberNeed(MemberNeed need)
    {
        context.MemberNeeds.Remove(need);
    }

    public void DeleteAvailabilitySlot(MemberAvailabilitySlot slot)
    {
        context.MemberAvailabilitySlots.Remove(slot);
    }

    public void DeletePhoto(Photo photo)
    {
        context.Photos.Remove(photo);
    }

    public void Update(Member member)
    {
        context.Entry(member).State = EntityState.Modified;
    }
}

