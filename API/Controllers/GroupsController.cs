using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class GroupsController(IUnitOfWork uow) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<GroupDto>>> GetGroups()
    {
        var memberId = User.GetMemberId();
        var groups = await uow.GroupRepository.GetGroupsAsync(memberId);

        return Ok(groups.Select(x => x.ToDto(memberId)));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<GroupDto>> GetGroup(int id)
    {
        var memberId = User.GetMemberId();
        var group = await uow.GroupRepository.GetGroupByIdAsync(id, memberId);

        if (group is null) return NotFound();

        return group.ToDto(memberId);
    }

    [HttpPost]
    public async Task<ActionResult<GroupDto>> CreateGroup(CreateGroupDto createGroupDto)
    {
        var memberId = User.GetMemberId();
        var name = createGroupDto.Name.Trim();

        if (await uow.GroupRepository.GroupNameExistsAsync(name))
        {
            return BadRequest("A group with this name already exists");
        }

        var group = new CommunityGroup
        {
            Name = name,
            Description = createGroupDto.Description.Trim(),
            Theme = string.IsNullOrWhiteSpace(createGroupDto.Theme) ? null : createGroupDto.Theme.Trim(),
            City = string.IsNullOrWhiteSpace(createGroupDto.City) ? null : createGroupDto.City.Trim(),
            CountryCode = string.IsNullOrWhiteSpace(createGroupDto.CountryCode) ? null : createGroupDto.CountryCode.Trim().ToUpper(),
            ModerationStatus = ModerationStatus.PendingApproval,
            OwnerMemberId = memberId,
            Members =
            [
                new CommunityGroupMember
                {
                    MemberId = memberId,
                    Role = GroupMemberRole.Owner
                }
            ]
        };

        uow.GroupRepository.AddGroup(group);

        if (!await uow.Complete()) return BadRequest("Failed to create group");

        var createdGroup = await uow.GroupRepository.GetGroupByIdAsync(group.Id, memberId);

        if (createdGroup is null) return BadRequest("Failed to load created group");

        return CreatedAtAction(nameof(GetGroup), new { id = group.Id }, createdGroup.ToDto(memberId));
    }

    [HttpPost("{id:int}/join")]
    public async Task<ActionResult<GroupDto>> JoinGroup(int id)
    {
        var memberId = User.GetMemberId();
        var group = await uow.GroupRepository.GetGroupByIdAsync(id, memberId);

        if (group is null) return NotFound();
        if (group.ModerationStatus != ModerationStatus.Approved) return BadRequest("Group must be approved before members can join");

        await uow.MessageRepository.GetOrCreateGroupConversationAsync(group);
        uow.GroupRepository.JoinGroup(group, memberId);

        if (!await uow.Complete()) return BadRequest("Failed to join group");

        var updatedGroup = await uow.GroupRepository.GetGroupByIdAsync(id, memberId);

        if (updatedGroup is null) return BadRequest("Failed to load group");

        return Ok(updatedGroup.ToDto(memberId));
    }

    [HttpPost("{id:int}/leave")]
    public async Task<ActionResult> LeaveGroup(int id)
    {
        var memberId = User.GetMemberId();
        var group = await uow.GroupRepository.GetGroupByIdAsync(id, memberId);

        if (group is null) return NotFound();
        if (group.OwnerMemberId == memberId) return BadRequest("Group owner cannot leave the group");
        if (!await uow.GroupRepository.IsGroupMemberAsync(id, memberId)) return BadRequest("You are not a member of this group");

        uow.GroupRepository.LeaveGroup(group, memberId);

        if (await uow.Complete()) return NoContent();

        return BadRequest("Failed to leave group");
    }
}
