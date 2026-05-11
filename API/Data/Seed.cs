using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using API.DTOs;
using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class Seed
{
    public static async Task SeedUsers(AppDbContext context)
    {
        if (await context.Users.AnyAsync()) return;

        var memberData = await File.ReadAllTextAsync("Data/UserSeedData.json");
        var options = new JsonSerializerOptions();
        options.Converters.Add(new JsonStringEnumConverter());
        var members = JsonSerializer.Deserialize<List<SeedUserDto>>(memberData, options);

        if (members == null)
        {
            Console.WriteLine("No members in seed data");
            return;
        }

        var categories = new Dictionary<string, ServiceCategory>(StringComparer.OrdinalIgnoreCase);

        ServiceCategory GetCategory(SeedServiceCategoryDto serviceCategory)
        {
            if (categories.TryGetValue(serviceCategory.Key, out var category))
            {
                return category;
            }

            category = new ServiceCategory
            {
                Key = serviceCategory.Key,
                Name = serviceCategory.Name
            };
            categories.Add(serviceCategory.Key, category);

            return category;
        }

        foreach (var member in members)
        {
            using var hmac = new HMACSHA512();
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
                Email = member.Email,
                ImageUrl = member.AvatarUrl,
                DisplayName = member.DisplayName,
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("Pa$$w0rd")),
                PasswordSalt = hmac.Key,
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

            context.Users.Add(user);
        }

        await context.SaveChangesAsync();
    }
}
