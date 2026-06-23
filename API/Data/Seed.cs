using System.Text.Json;
using System.Text.Json.Serialization;
using API.DTOs;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class Seed
{
    private const string AdminMemberId = "admin-user-id";
    private const string StartingBalanceNote = "Seed starting balance";

    public static async Task SeedUsers(UserManager<AppUser> userManager, AppDbContext context)
    {
        if (await userManager.Users.AnyAsync())
        {
            await EnsureAdminUser(userManager);
            await SeedDemoExchangeData(context);
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
        await SeedDemoExchangeData(context);
    }

    private static async Task EnsureAdminUser(UserManager<AppUser> userManager)
    {
        const string adminEmail = "admin@test.com";
        var admin = await userManager.FindByEmailAsync(adminEmail);

        if (admin is null)
        {
            admin = new AppUser
            {
                Id = AdminMemberId,
                DisplayName = "Admin",
                Email = adminEmail,
                UserName = adminEmail,
                Member = new Member
                {
                    Id = AdminMemberId,
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

    private static async Task SeedDemoExchangeData(AppDbContext context)
    {
        var categories = await context.ServiceCategories.ToDictionaryAsync(x => x.Key, StringComparer.OrdinalIgnoreCase);
        var members = await context.Members
            .Where(x => x.Id != AdminMemberId)
            .ToDictionaryAsync(x => x.Id);

        if (members.Count == 0) return;

        await SeedStartingCredits(context, categories, members);
        await SeedDemoTasks(context, categories);
        await SeedDemoGroups(context);
    }

    private static async Task SeedStartingCredits(
        AppDbContext context,
        IReadOnlyDictionary<string, ServiceCategory> categories,
        IReadOnlyDictionary<string, Member> members)
    {
        var adminExists = await context.Members.AnyAsync(x => x.Id == AdminMemberId);
        if (!adminExists || !categories.TryGetValue("mentoring", out var category)) return;

        foreach (var member in members.Values)
        {
            var alreadyGranted = await context.TimeTransactions
                .AnyAsync(x => x.ToMemberId == member.Id && x.Note == StartingBalanceNote);

            if (alreadyGranted) continue;

            var grantTask = new TimeTask
            {
                Title = $"Welcome time-credit grant for {member.DisplayName}",
                Description = "Initial seed credit grant so demo members can post and complete time exchanges.",
                ServiceCategoryId = category.Id,
                EstimatedHours = 5,
                LocationMode = TaskLocationMode.Remote,
                City = member.City,
                CountryCode = member.CountryCode,
                FormattedAddress = member.City is null || member.CountryCode is null ? null : $"{member.City}, {member.CountryCode}",
                CreatedAtUtc = new DateTime(2026, 6, 1, 9, 0, 0, DateTimeKind.Utc),
                UpdatedAtUtc = new DateTime(2026, 6, 1, 9, 0, 0, DateTimeKind.Utc),
                CompletedAtUtc = new DateTime(2026, 6, 1, 9, 0, 0, DateTimeKind.Utc),
                Status = TimeTaskStatus.Completed,
                PostedByMemberId = AdminMemberId,
                AcceptedByMemberId = member.Id
            };

            context.TimeTasks.Add(grantTask);
            await context.SaveChangesAsync();

            context.TimeTransactions.Add(new TimeTransaction
            {
                TimeTaskId = grantTask.Id,
                FromMemberId = AdminMemberId,
                ToMemberId = member.Id,
                Hours = 5,
                CreatedAtUtc = new DateTime(2026, 6, 1, 9, 0, 0, DateTimeKind.Utc),
                Note = StartingBalanceNote
            });

            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedDemoTasks(AppDbContext context, IReadOnlyDictionary<string, ServiceCategory> categories)
    {
        var tasks = new[]
        {
            new DemoTaskSeed("Tune up my city bike brakes", "My commuter bike needs a brake adjustment and a quick safety check before I use it daily again.", "bike-repair", 2, TaskLocationMode.InPerson, "Cluj-Napoca", "RO", "Cluj-Napoca, Romania", 46.7712, 23.6236, "ana-id", new DateTime(2026, 7, 8, 15, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 10, 10, 0, 0, DateTimeKind.Utc)),
            new DemoTaskSeed("Help prepare vegetarian meals for the week", "I would like help planning and cooking a few simple vegetarian meals that can be stored for busy work days.", "meal-prep", 3, TaskLocationMode.InPerson, "Brasov", "RO", "Brasov, Romania", 45.6579, 25.6012, "matei-id", new DateTime(2026, 7, 10, 12, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 11, 11, 30, 0, DateTimeKind.Utc)),
            new DemoTaskSeed("Clean apartment windows before the weekend", "I need help cleaning several apartment windows. I can provide basic supplies and flexible timing.", "window-cleaning", 2, TaskLocationMode.InPerson, "Bucharest", "RO", "Bucharest, Romania", 44.4268, 26.1025, "elena-id", new DateTime(2026, 7, 12, 9, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 12, 8, 45, 0, DateTimeKind.Utc)),
            new DemoTaskSeed("Scan and organize a folder of documents", "I have a small folder of paperwork that needs scanning, naming, and organizing into clear digital folders.", "document-scanning", 2, TaskLocationMode.Either, "Timisoara", "RO", "Timisoara, Romania", 45.7489, 21.2087, "darius-id", new DateTime(2026, 7, 15, 14, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 13, 13, 20, 0, DateTimeKind.Utc)),
            new DemoTaskSeed("Mount two wall shelves", "I need someone comfortable with basic tools to help mount two lightweight wall shelves safely.", "shelf-installation", 3, TaskLocationMode.InPerson, "Iasi", "RO", "Iasi, Romania", 47.1585, 27.6014, "irina-id", new DateTime(2026, 7, 18, 10, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 14, 15, 10, 0, DateTimeKind.Utc)),
            new DemoTaskSeed("Back up phone photos to cloud storage", "I would like help moving phone photos into cloud storage and checking that the albums are easy to find later.", "photo-backup", 1, TaskLocationMode.Either, "Sibiu", "RO", "Sibiu, Romania", 45.7983, 24.1256, "sorin-id", new DateTime(2026, 7, 20, 8, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 15, 9, 40, 0, DateTimeKind.Utc))
        };

        foreach (var task in tasks)
        {
            var exists = await context.TimeTasks.AnyAsync(x => x.Title == task.Title && x.PostedByMemberId == task.PostedByMemberId);
            if (exists || !categories.TryGetValue(task.CategoryKey, out var category)) continue;

            context.TimeTasks.Add(new TimeTask
            {
                Title = task.Title,
                Description = task.Description,
                ServiceCategoryId = category.Id,
                EstimatedHours = task.EstimatedHours,
                LocationMode = task.LocationMode,
                City = task.City,
                CountryCode = task.CountryCode,
                FormattedAddress = task.FormattedAddress,
                PlaceId = $"seed-{task.City.ToLowerInvariant()}-{task.CategoryKey}",
                Latitude = task.Latitude,
                Longitude = task.Longitude,
                CreatedAtUtc = task.CreatedAtUtc,
                DueAtUtc = task.DueAtUtc,
                Status = TimeTaskStatus.Open,
                PostedByMemberId = task.PostedByMemberId
            });
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedDemoGroups(AppDbContext context)
    {
        var groups = new[]
        {
            new DemoGroupSeed("Cluj Practical Help Circle", "A local group for small repairs, errands, and neighbourly exchanges around Cluj.", "Repairs and errands", "Cluj-Napoca", "RO", "ana-id", new[] { "ana-id", "matei-id", "darius-id" }),
            new DemoGroupSeed("Bucharest Study and Home Support", "A public space for tutoring, study planning, household help, and trusted time exchanges.", "Study and home", "Bucharest", "RO", "elena-id", new[] { "elena-id", "sorin-id", "irina-id" }),
            new DemoGroupSeed("Garden and Shared Tools Exchange", "People who want to share tools, garden skills, compost ideas, and seasonal cleanup help.", "Gardening", "Timisoara", "RO", "darius-id", new[] { "darius-id", "ana-id", "irina-id" })
        };

        foreach (var groupSeed in groups)
        {
            var exists = await context.CommunityGroups.AnyAsync(x => x.Name == groupSeed.Name);
            if (exists) continue;

            var createdAtUtc = new DateTime(2026, 6, 8, 12, 0, 0, DateTimeKind.Utc);
            var group = new CommunityGroup
            {
                Name = groupSeed.Name,
                Description = groupSeed.Description,
                Theme = groupSeed.Theme,
                City = groupSeed.City,
                CountryCode = groupSeed.CountryCode,
                ModerationStatus = ModerationStatus.Approved,
                CreatedAtUtc = createdAtUtc,
                OwnerMemberId = groupSeed.OwnerMemberId,
                ReviewedByMemberId = AdminMemberId,
                ReviewedAtUtc = createdAtUtc,
                Members = groupSeed.MemberIds.Distinct().Select(memberId => new CommunityGroupMember
                {
                    MemberId = memberId,
                    Role = memberId == groupSeed.OwnerMemberId ? GroupMemberRole.Owner : GroupMemberRole.Member,
                    JoinedAtUtc = createdAtUtc
                }).ToList(),
                Conversation = new Conversation
                {
                    Type = ConversationType.Group,
                    Title = groupSeed.Name,
                    CreatedAtUtc = createdAtUtc,
                    Participants = groupSeed.MemberIds.Distinct().Select(memberId => new ConversationParticipant
                    {
                        MemberId = memberId,
                        JoinedAtUtc = createdAtUtc
                    }).ToList()
                }
            };

            context.CommunityGroups.Add(group);
        }

        await context.SaveChangesAsync();
    }

    private record DemoTaskSeed(
        string Title,
        string Description,
        string CategoryKey,
        int EstimatedHours,
        TaskLocationMode LocationMode,
        string City,
        string CountryCode,
        string FormattedAddress,
        double Latitude,
        double Longitude,
        string PostedByMemberId,
        DateTime DueAtUtc,
        DateTime CreatedAtUtc);

    private record DemoGroupSeed(
        string Name,
        string Description,
        string Theme,
        string City,
        string CountryCode,
        string OwnerMemberId,
        string[] MemberIds);
}
