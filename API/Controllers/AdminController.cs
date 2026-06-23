using API.DTOs;
using API.Entities;
using API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize(Policy = "RequireAdminRole")]
public class AdminController(UserManager<AppUser> userManager) : BaseApiController
{
    private static readonly string[] AllowedRoles = ["Member", "Moderator", "Admin"];

    [HttpGet("users-with-roles")]
    public async Task<ActionResult<IReadOnlyList<AdminUserDto>>> GetUsersWithRoles()
    {
        var users = userManager.Users
            .OrderBy(x => x.DisplayName)
            .ToList();

        var usersWithRoles = new List<AdminUserDto>();

        foreach (var user in users)
        {
            usersWithRoles.Add(new AdminUserDto
            {
                Id = user.Id,
                DisplayName = user.DisplayName,
                Email = user.Email!,
                ImageUrl = user.ImageUrl,
                Roles = (await userManager.GetRolesAsync(user)).ToList()
            });
        }

        return usersWithRoles;
    }

    [HttpPost("edit-roles/{userId}")]
    public async Task<ActionResult<IReadOnlyList<string>>> EditRoles(string userId, [FromQuery] string roles)
    {
        var selectedRoles = roles.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (selectedRoles.Except(AllowedRoles).Any())
        {
            return BadRequest("One or more selected roles are invalid");
        }

        var user = await userManager.FindByIdAsync(userId);

        if (user is null)
        {
            return NotFound("User not found");
        }

        var currentRoles = await userManager.GetRolesAsync(user);
        var currentUserId = User.GetMemberId();

        if (user.Id == currentUserId && currentRoles.Contains("Admin") && !selectedRoles.Contains("Admin"))
        {
            return BadRequest("You cannot remove your own Admin role");
        }

        var removeResult = await userManager.RemoveFromRolesAsync(user, currentRoles.Except(selectedRoles));

        if (!removeResult.Succeeded)
        {
            return BadRequest(removeResult.Errors);
        }

        var addResult = await userManager.AddToRolesAsync(user, selectedRoles.Except(currentRoles));

        if (!addResult.Succeeded)
        {
            return BadRequest(addResult.Errors);
        }

        return Ok(await userManager.GetRolesAsync(user));
    }
}
