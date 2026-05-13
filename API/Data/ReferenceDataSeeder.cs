using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public static class ReferenceDataSeeder
{
    public static async Task SeedServiceCategories(AppDbContext context)
    {
        var existingKeys = await context.ServiceCategories
            .Select(x => x.Key)
            .ToListAsync();

        var existingKeySet = existingKeys.ToHashSet(StringComparer.OrdinalIgnoreCase);

        var categories = new[]
        {
            new ServiceCategory { Key = "app-setup", Name = "App setup" },
            new ServiceCategory { Key = "basic-repairs", Name = "Basic repairs" },
            new ServiceCategory { Key = "bike-repair", Name = "Bike repair" },
            new ServiceCategory { Key = "compost-setup", Name = "Compost setup" },
            new ServiceCategory { Key = "computer-setup", Name = "Computer setup" },
            new ServiceCategory { Key = "document-scanning", Name = "Document scanning" },
            new ServiceCategory { Key = "dog-walking", Name = "Dog walking" },
            new ServiceCategory { Key = "electrical-advice", Name = "Electrical advice" },
            new ServiceCategory { Key = "flyer-design", Name = "Flyer design" },
            new ServiceCategory { Key = "garden-cleanup", Name = "Garden cleanup" },
            new ServiceCategory { Key = "gardening", Name = "Gardening" },
            new ServiceCategory { Key = "grocery-pickup", Name = "Grocery pickup" },
            new ServiceCategory { Key = "home-cooking", Name = "Home cooking" },
            new ServiceCategory { Key = "language-practice", Name = "Language practice" },
            new ServiceCategory { Key = "math-tutoring", Name = "Math tutoring" },
            new ServiceCategory { Key = "meal-prep", Name = "Meal prep" },
            new ServiceCategory { Key = "mentoring", Name = "Mentoring" },
            new ServiceCategory { Key = "moving-help", Name = "Moving help" },
            new ServiceCategory { Key = "photo-backup", Name = "Photo backup" },
            new ServiceCategory { Key = "shelf-installation", Name = "Shelf installation" },
            new ServiceCategory { Key = "spanish-practice", Name = "Spanish practice" },
            new ServiceCategory { Key = "study-planning", Name = "Study planning" },
            new ServiceCategory { Key = "window-cleaning", Name = "Window cleaning" }
        };

        var missingCategories = categories
            .Where(x => !existingKeySet.Contains(x.Key))
            .ToList();

        if (missingCategories.Count == 0) return;

        context.ServiceCategories.AddRange(missingCategories);
        await context.SaveChangesAsync();
    }
}
