using API.Entities;

namespace API.Interfaces;

public interface IMemberRepository
{
    void Update(Member member);
    Task<bool> SaveAllAsync();
    Task<IReadOnlyList<Member>> GetMembersAsync();
    Task<Member?> GetMemberByIdAsync(string id);
    Task<IReadOnlyList<Photo>> GetPhotosForMemberAsync(string memberId);
    Task<Member?> GetMemberForUpdateAsync(string id);
    Task<Member?> GetMemberWithServicePreferencesForUpdateAsync(string id);
    Task<bool> ServiceCategoryExistsAsync(int serviceCategoryId);
    Task<MemberSkill?> GetMemberSkillForUpdateAsync(string memberId, int skillId);
    Task<MemberNeed?> GetMemberNeedForUpdateAsync(string memberId, int needId);
    Task<Member?> GetMemberWithAvailabilityForUpdateAsync(string id);
    Task<MemberAvailabilitySlot?> GetAvailabilitySlotForUpdateAsync(string memberId, int slotId);
    void DeleteMemberSkill(MemberSkill skill);
    void DeleteMemberNeed(MemberNeed need);
    void DeleteAvailabilitySlot(MemberAvailabilitySlot slot);
}
