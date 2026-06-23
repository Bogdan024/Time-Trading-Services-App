using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class MembersController(IUnitOfWork uow, IPhotoService photoService) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Member>>> GetMembers()
    {
        return Ok(await uow.MemberRepository.GetMembersAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Member>> GetMember(string id)
    {
        var member = await uow.MemberRepository.GetMemberByIdAsync(id);

        if (member is null) return NotFound();

        return member;
    }

    [HttpGet("{id}/photos")]
    public async Task<ActionResult<IReadOnlyList<Photo>>> GetMemberPhotos(string id)
    {
        return Ok(await uow.MemberRepository.GetPhotosForMemberAsync(id));
    }

    [HttpPut]
    public async Task<ActionResult> UpdateMember(MemberUpdateDto memberUpdateDto)
    {
        var member = await uow.MemberRepository.GetMemberForUpdateAsync(User.GetMemberId());

        if (member is null) return BadRequest("Could not get member");

        member.DisplayName = memberUpdateDto.DisplayName ?? member.DisplayName;
        member.About = memberUpdateDto.About ?? member.About;
        member.City = memberUpdateDto.City ?? member.City;
        member.CountryCode = memberUpdateDto.CountryCode ?? member.CountryCode;
        member.IsProfilePublic = memberUpdateDto.IsProfilePublic ?? member.IsProfilePublic;
        member.User.DisplayName = member.DisplayName;

        uow.MemberRepository.Update(member);

        if (await uow.Complete()) return NoContent();

        return BadRequest("Failed to update member");
    }

    [HttpPost("skills")]
    public async Task<ActionResult<Member>> AddSkill(MemberServiceCategoryEditDto serviceCategoryEditDto)
    {
        var memberId = User.GetMemberId();

        if (!await uow.MemberRepository.ServiceCategoryExistsAsync(serviceCategoryEditDto.ServiceCategoryId))
        {
            return BadRequest("Service category does not exist");
        }

        var member = await uow.MemberRepository.GetMemberWithServicePreferencesForUpdateAsync(memberId);

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

        if (!await uow.Complete()) return BadRequest("Failed to add skill");

        return await GetUpdatedMember(memberId);
    }

    [HttpDelete("skills/{skillId:int}")]
    public async Task<ActionResult<Member>> DeleteSkill(int skillId)
    {
        var memberId = User.GetMemberId();
        var skill = await uow.MemberRepository.GetMemberSkillForUpdateAsync(memberId, skillId);

        if (skill is null) return NotFound();

        uow.MemberRepository.DeleteMemberSkill(skill);

        if (!await uow.Complete()) return BadRequest("Failed to remove skill");

        return await GetUpdatedMember(memberId);
    }

    [HttpPost("needs")]
    public async Task<ActionResult<Member>> AddNeed(MemberServiceCategoryEditDto serviceCategoryEditDto)
    {
        var memberId = User.GetMemberId();

        if (!await uow.MemberRepository.ServiceCategoryExistsAsync(serviceCategoryEditDto.ServiceCategoryId))
        {
            return BadRequest("Service category does not exist");
        }

        var member = await uow.MemberRepository.GetMemberWithServicePreferencesForUpdateAsync(memberId);

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

        if (!await uow.Complete()) return BadRequest("Failed to add need");

        return await GetUpdatedMember(memberId);
    }

    [HttpDelete("needs/{needId:int}")]
    public async Task<ActionResult<Member>> DeleteNeed(int needId)
    {
        var memberId = User.GetMemberId();
        var need = await uow.MemberRepository.GetMemberNeedForUpdateAsync(memberId, needId);

        if (need is null) return NotFound();

        uow.MemberRepository.DeleteMemberNeed(need);

        if (!await uow.Complete()) return BadRequest("Failed to remove need");

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

        var member = await uow.MemberRepository.GetMemberWithAvailabilityForUpdateAsync(memberId);

        if (member is null) return BadRequest("Could not get member");

        if (member.AvailabilitySlots.Any(x =>
            x.DayOfWeek == availabilitySlotEditDto.DayOfWeek &&
            x.StartHour == availabilitySlotEditDto.StartHour &&
            x.EndHour == availabilitySlotEditDto.EndHour))
        {
            return BadRequest("Availability slot already exists");
        }

        member.AvailabilitySlots.Add(new MemberAvailabilitySlot
        {
            DayOfWeek = availabilitySlotEditDto.DayOfWeek,
            StartHour = availabilitySlotEditDto.StartHour,
            EndHour = availabilitySlotEditDto.EndHour,
            Mode = availabilitySlotEditDto.Mode
        });

        if (!await uow.Complete()) return BadRequest("Failed to add availability slot");

        return await GetUpdatedMember(memberId);
    }

    [HttpDelete("availability/{slotId:int}")]
    public async Task<ActionResult<Member>> DeleteAvailabilitySlot(int slotId)
    {
        var memberId = User.GetMemberId();
        var slot = await uow.MemberRepository.GetAvailabilitySlotForUpdateAsync(memberId, slotId);

        if (slot is null) return NotFound();

        uow.MemberRepository.DeleteAvailabilitySlot(slot);

        if (!await uow.Complete()) return BadRequest("Failed to remove availability slot");

        return await GetUpdatedMember(memberId);
    }

    [HttpPost("avatar")]
    public async Task<ActionResult<Member>> UpdateAvatar([FromForm] IFormFile file)
    {
        if (file is null || file.Length == 0) return BadRequest("No file was uploaded");
        if (!file.ContentType.StartsWith("image/")) return BadRequest("Only image files are allowed");

        var memberId = User.GetMemberId();
        var member = await uow.MemberRepository.GetMemberForAvatarUpdateAsync(memberId);

        if (member is null) return BadRequest("Could not get member");

        var uploadResult = await photoService.UploadAvatarAsync(file);

        if (uploadResult.Error is not null) return BadRequest(uploadResult.Error.Message);

        var oldAvatar = member.Photos.FirstOrDefault(x => x.Url == member.AvatarUrl);
        var oldAvatarPublicId = oldAvatar?.PublicId;

        if (oldAvatar is not null)
        {
            uow.MemberRepository.DeletePhoto(oldAvatar);
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

        if (!await uow.Complete()) return BadRequest("Failed to update avatar");

        if (!string.IsNullOrWhiteSpace(oldAvatarPublicId))
        {
            await photoService.DeletePhotoAsync(oldAvatarPublicId);
        }

        return await GetUpdatedMember(memberId);
    }

    private async Task<ActionResult<Member>> GetUpdatedMember(string memberId)
    {
        var updatedMember = await uow.MemberRepository.GetMemberByIdAsync(memberId);

        if (updatedMember is null) return BadRequest("Could not load updated member");

        return Ok(updatedMember);
    }
}
