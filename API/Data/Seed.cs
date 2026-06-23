using System.Text.Json;
using System.Text.Json.Serialization;
using API.DTOs;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class Seed
{
    public static async Task SeedUsers(UserManager<AppUser> userManager, AppDbContext context)
    {
        if (await userManager.Users.AnyAsync())
        {
            await EnsureAdminUser(userManager);
            return;
        }

        var memberData = await File.ReadAllTextAsync("Data/UserSeedData.json");
        var options = new JsonSerializerOptions();
        options.Converters.Add(new JsonStringEnumConverter());
        var members = JsonSerializer.Deserialize<List<SeedUserDto>>(memberData, options);

        if (members == null)
        {
            Console.WriteLine("No members in seed data");
            return;
        }

        var existingCategories = await context.ServiceCategories.ToListAsync();
        var categories = existingCategories.ToDictionary(x => x.Key, StringComparer.OrdinalIgnoreCase);

        ServiceCategory GetCategory(SeedServiceCategoryDto serviceCategory)
        {
            if (!categories.TryGetValue(serviceCategory.Key, out var category))
            {
                throw new InvalidOperationException(
                    $"Seed user data references unknown service category '{serviceCategory.Key}'. Add it to ReferenceDataSeeder first.");
            }

            return category;
        }

        foreach (var member in members)
        {
            var memberProfile = new Member
            {
                Id = member.Id,
                DisplayName = member.DisplayName,
                AvatarUrl = member.AvatarUrl,
                About = member.About,
                City = member.City,
                CountryCode = member.CountryCode,
                CreatedAtUtc = member.CreatedAtUtc,
                LastActiveAtUtc = member.LastActiveAtUtc
            };

            foreach (var skill in member.OfferedSkills)
            {
                memberProfile.OfferedSkills.Add(new MemberSkill
                {
                    ServiceCategory = GetCategory(skill),
                    Note = skill.Note
                });
            }

            foreach (var need in member.NeedsHelpWith)
            {
                memberProfile.NeedsHelpWith.Add(new MemberNeed
                {
                    ServiceCategory = GetCategory(need),
                    Note = need.Note
                });
            }

            foreach (var slot in member.AvailabilitySlots)
            {
                memberProfile.AvailabilitySlots.Add(new MemberAvailabilitySlot
                {
                    DayOfWeek = slot.DayOfWeek,
                    StartHour = slot.StartHour,
                    EndHour = slot.EndHour,
                    Mode = Enum.Parse<AvailabilityMode>(slot.Mode, ignoreCase: true)
                });
            }

            var user = new AppUser
            {
                Id = member.Id,
                Email = member.Email.ToLowerInvariant(),
                UserName = member.Email.ToLowerInvariant(),
                ImageUrl = member.AvatarUrl,
                DisplayName = member.DisplayName,
                Member = memberProfile
            };

            if (!string.IsNullOrWhiteSpace(member.AvatarUrl))
            {
                user.Member.Photos.Add(new Photo
                {
                    Url = member.AvatarUrl,
                    MemberId = member.Id
                });
            }

            var result = await userManager.CreateAsync(user, "Pa$$w0rd");

            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Failed to seed user '{member.Email}': {string.Join(", ", result.Errors.Select(x => x.Description))}");
            }

            await userManager.AddToRoleAsync(user, "Member");
        }

        await EnsureAdminUser(userManager);
    }

    private static async Task EnsureAdminUser(UserManager<AppUser> userManager)
    {
        const string adminEmail = "admin@test.com";
        var admin = await userManager.FindByEmailAsync(adminEmail);

        if (admin is null)
        {
            admin = new AppUser
            {
                Id = "admin-user-id",
                DisplayName = "Admin",
                Email = adminEmail,
                UserName = adminEmail,
                Member = new Member
                {
                    Id = "admin-user-id",
                    DisplayName = "Admin",
                    City = "Bucharest",
                    CountryCode = "RO",
                    About = "Platform administrator profile.",
                    IsProfilePublic = false
                }
            };

            var result = await userManager.CreateAsync(admin, "Pa$$w0rd");

            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Failed to seed admin user: {string.Join(", ", result.Errors.Select(x => x.Description))}");
            }
        }

        var roles = new[] { "Member", "Moderator", "Admin" };
        var currentRoles = await userManager.GetRolesAsync(admin);
        var missingRoles = roles.Except(currentRoles).ToArray();

        if (missingRoles.Length > 0)
        {
            await userManager.AddToRolesAsync(admin, missingRoles);
        }
    }
}
