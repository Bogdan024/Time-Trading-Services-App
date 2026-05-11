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

    public async Task<IReadOnlyList<Member>> GetMembersAsync()
    {
        return await context.Members
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

    public async Task<bool> SaveAllAsync()
    {
        return await context.SaveChangesAsync() > 0;
    }

    public void Update(Member member)
    {
        context.Entry(member).State = EntityState.Modified;
    }
}
