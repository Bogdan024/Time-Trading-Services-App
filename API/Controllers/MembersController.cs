using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class MembersController(IMemberRepository memberRepository, IPhotoService photoService) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Member>>> GetMembers()
    {
        return Ok(await memberRepository.GetMembersAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Member>> GetMember(string id)
    {
        var member = await memberRepository.GetMemberByIdAsync(id);

        if (member is null) return NotFound();

        return member;
    }

    [HttpGet("{id}/photos")]
    public async Task<ActionResult<IReadOnlyList<Photo>>> GetMemberPhotos(string id)
    {
        return Ok(await memberRepository.GetPhotosForMemberAsync(id));
    }

    [HttpPut]
    public async Task<ActionResult> UpdateMember(MemberUpdateDto memberUpdateDto)
    {
        var member = await memberRepository.GetMemberForUpdateAsync(User.GetMemberId());

        if (member is null) return BadRequest("Could not get member");

        member.DisplayName = memberUpdateDto.DisplayName ?? member.DisplayName;
        member.About = memberUpdateDto.About ?? member.About;
        member.City = memberUpdateDto.City ?? member.City;
        member.CountryCode = memberUpdateDto.CountryCode ?? member.CountryCode;
        member.IsProfilePublic = memberUpdateDto.IsProfilePublic ?? member.IsProfilePublic;
        member.User.DisplayName = member.DisplayName;

        memberRepository.Update(member);

        if (await memberRepository.SaveAllAsync()) return NoContent();

        return BadRequest("Failed to update member");
    }

    [HttpPost("skills")]
    public async Task<ActionResult<Member>> AddSkill(MemberServiceCategoryEditDto serviceCategoryEditDto)
    {
        var memberId = User.GetMemberId();

        if (!await memberRepository.ServiceCategoryExistsAsync(serviceCategoryEditDto.ServiceCategoryId))
        {
            return BadRequest("Service category does not exist");
        }

        var member = await memberRepository.GetMemberWithServicePreferencesForUpdateAsync(memberId);

        if (member is null) return BadRequest("Could not get member");

        if (member.OfferedSkills.Any(x => x.ServiceCategoryId == serviceCategoryEditDto.ServiceCategoryId))
        {
            return BadRequest("Skill already added");
        }

        member.OfferedSkills.Add(new MemberSkill
        {
            ServiceCategoryId = serviceCategoryEditDto.ServiceCategoryId,
            Note = serviceCategoryEditDto.Note
        });

        if (!await memberRepository.SaveAllAsync()) return BadRequest("Failed to add skill");

        return await GetUpdatedMember(memberId);
    }

    [HttpDelete("skills/{skillId:int}")]
    public async Task<ActionResult<Member>> DeleteSkill(int skillId)
    {
        var memberId = User.GetMemberId();
        var skill = await memberRepository.GetMemberSkillForUpdateAsync(memberId, skillId);

        if (skill is null) return NotFound();

        memberRepository.DeleteMemberSkill(skill);

        if (!await memberRepository.SaveAllAsync()) return BadRequest("Failed to remove skill");

        return await GetUpdatedMember(memberId);
    }

    [HttpPost("needs")]
    public async Task<ActionResult<Member>> AddNeed(MemberServiceCategoryEditDto serviceCategoryEditDto)
    {
        var memberId = User.GetMemberId();

        if (!await memberRepository.ServiceCategoryExistsAsync(serviceCategoryEditDto.ServiceCategoryId))
        {
            return BadRequest("Service category does not exist");
        }

        var member = await memberRepository.GetMemberWithServicePreferencesForUpdateAsync(memberId);

        if (member is null) return BadRequest("Could not get member");

        if (member.NeedsHelpWith.Any(x => x.ServiceCategoryId == serviceCategoryEditDto.ServiceCategoryId))
        {
            return BadRequest("Need already added");
        }

        member.NeedsHelpWith.Add(new MemberNeed
        {
            ServiceCategoryId = serviceCategoryEditDto.ServiceCategoryId,
            Note = serviceCategoryEditDto.Note
        });

        if (!await memberRepository.SaveAllAsync()) return BadRequest("Failed to add need");

        return await GetUpdatedMember(memberId);
    }

    [HttpDelete("needs/{needId:int}")]
    public async Task<ActionResult<Member>> DeleteNeed(int needId)
    {
        var memberId = User.GetMemberId();
        var need = await memberRepository.GetMemberNeedForUpdateAsync(memberId, needId);

        if (need is null) return NotFound();

        memberRepository.DeleteMemberNeed(need);

        if (!await memberRepository.SaveAllAsync()) return BadRequest("Failed to remove need");

        return await GetUpdatedMember(memberId);
    }

    [HttpPost("availability")]
    public async Task<ActionResult<Member>> AddAvailabilitySlot(MemberAvailabilitySlotEditDto availabilitySlotEditDto)
    {
        var memberId = User.GetMemberId();

        if (availabilitySlotEditDto.EndHour <= availabilitySlotEditDto.StartHour)
        {
            return BadRequest("End hour must be after start hour");
        }

        var member = await memberRepository.GetMemberWithAvailabilityForUpdateAsync(memberId);

        if (member is null) return BadRequest("Could not get member");

        member.AvailabilitySlots.Add(new MemberAvailabilitySlot
        {
            DayOfWeek = availabilitySlotEditDto.DayOfWeek,
            StartHour = availabilitySlotEditDto.StartHour,
            EndHour = availabilitySlotEditDto.EndHour,
            Mode = availabilitySlotEditDto.Mode
        });

        if (!await memberRepository.SaveAllAsync()) return BadRequest("Failed to add availability slot");

        return await GetUpdatedMember(memberId);
    }

    [HttpDelete("availability/{slotId:int}")]
    public async Task<ActionResult<Member>> DeleteAvailabilitySlot(int slotId)
    {
        var memberId = User.GetMemberId();
        var slot = await memberRepository.GetAvailabilitySlotForUpdateAsync(memberId, slotId);

        if (slot is null) return NotFound();

        memberRepository.DeleteAvailabilitySlot(slot);

        if (!await memberRepository.SaveAllAsync()) return BadRequest("Failed to remove availability slot");

        return await GetUpdatedMember(memberId);
    }

    [HttpPost("avatar")]
    public async Task<ActionResult<Member>> UpdateAvatar([FromForm] IFormFile file)
    {
        if (file is null || file.Length == 0) return BadRequest("No file was uploaded");
        if (!file.ContentType.StartsWith("image/")) return BadRequest("Only image files are allowed");

        var memberId = User.GetMemberId();
        var member = await memberRepository.GetMemberForAvatarUpdateAsync(memberId);

        if (member is null) return BadRequest("Could not get member");

        var uploadResult = await photoService.UploadAvatarAsync(file);

        if (uploadResult.Error is not null) return BadRequest(uploadResult.Error.Message);

        var oldAvatar = member.Photos.FirstOrDefault(x => x.Url == member.AvatarUrl);
        var oldAvatarPublicId = oldAvatar?.PublicId;

        if (oldAvatar is not null)
        {
            memberRepository.DeletePhoto(oldAvatar);
        }

        var photo = new Photo
        {
            Url = uploadResult.SecureUrl.AbsoluteUri,
            PublicId = uploadResult.PublicId,
            MemberId = memberId
        };

        member.AvatarUrl = photo.Url;
        member.User.ImageUrl = photo.Url;
        member.Photos.Add(photo);

        if (!await memberRepository.SaveAllAsync()) return BadRequest("Failed to update avatar");

        if (!string.IsNullOrWhiteSpace(oldAvatarPublicId))
        {
            await photoService.DeletePhotoAsync(oldAvatarPublicId);
        }

        return await GetUpdatedMember(memberId);
    }

    private async Task<ActionResult<Member>> GetUpdatedMember(string memberId)
    {
        var updatedMember = await memberRepository.GetMemberByIdAsync(memberId);

        if (updatedMember is null) return BadRequest("Could not load updated member");

        return Ok(updatedMember);
    }
}
