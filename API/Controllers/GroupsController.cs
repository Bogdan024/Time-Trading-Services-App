using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class GroupsController(IGroupRepository groupRepository, IMessageRepository messageRepository) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<GroupDto>>> GetGroups()
    {
        var memberId = User.GetMemberId();
        var groups = await groupRepository.GetGroupsAsync(memberId);

        return Ok(groups.Select(x => x.ToDto(memberId)));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<GroupDto>> GetGroup(int id)
    {
        var memberId = User.GetMemberId();
        var group = await groupRepository.GetGroupByIdAsync(id, memberId);

        if (group is null) return NotFound();

        return group.ToDto(memberId);
    }

    [HttpPost]
    public async Task<ActionResult<GroupDto>> CreateGroup(CreateGroupDto createGroupDto)
    {
        var memberId = User.GetMemberId();
        var name = createGroupDto.Name.Trim();

        if (await groupRepository.GroupNameExistsAsync(name))
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
            IsPublic = createGroupDto.IsPublic,
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

        groupRepository.AddGroup(group);
        await messageRepository.GetOrCreateGroupConversationAsync(group);

        if (!await groupRepository.SaveAllAsync()) return BadRequest("Failed to create group");

        var createdGroup = await groupRepository.GetGroupByIdAsync(group.Id, memberId);

        if (createdGroup is null) return BadRequest("Failed to load created group");

        return CreatedAtAction(nameof(GetGroup), new { id = group.Id }, createdGroup.ToDto(memberId));
    }

    [HttpPost("{id:int}/join")]
    public async Task<ActionResult<GroupDto>> JoinGroup(int id)
    {
        var memberId = User.GetMemberId();
        var group = await groupRepository.GetGroupByIdAsync(id, memberId);

        if (group is null) return NotFound();

        await messageRepository.GetOrCreateGroupConversationAsync(group);
        groupRepository.JoinGroup(group, memberId);

        if (!await groupRepository.SaveAllAsync()) return BadRequest("Failed to join group");

        var updatedGroup = await groupRepository.GetGroupByIdAsync(id, memberId);

        if (updatedGroup is null) return BadRequest("Failed to load group");

        return Ok(updatedGroup.ToDto(memberId));
    }

    [HttpPost("{id:int}/leave")]
    public async Task<ActionResult> LeaveGroup(int id)
    {
        var memberId = User.GetMemberId();
        var group = await groupRepository.GetGroupByIdAsync(id, memberId);

        if (group is null) return NotFound();
        if (group.OwnerMemberId == memberId) return BadRequest("Group owner cannot leave the group");
        if (!await groupRepository.IsGroupMemberAsync(id, memberId)) return BadRequest("You are not a member of this group");

        groupRepository.LeaveGroup(group, memberId);

        if (await groupRepository.SaveAllAsync()) return NoContent();

        return BadRequest("Failed to leave group");
    }
}

