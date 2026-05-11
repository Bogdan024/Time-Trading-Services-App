namespace API.DTOs;

public class SeedUserDto
{
    public required string Id { get; set; }
    public required string Email { get; set; }
    public required string DisplayName { get; set; }
    public string? AvatarUrl { get; set; }
    public string? About { get; set; }
    public string? City { get; set; }
    public string? CountryCode { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime LastActiveAtUtc { get; set; }
    public List<SeedServiceCategoryDto> OfferedSkills { get; set; } = [];
    public List<SeedServiceCategoryDto> NeedsHelpWith { get; set; } = [];
    public List<SeedAvailabilitySlotDto> AvailabilitySlots { get; set; } = [];
}

public class SeedServiceCategoryDto
{
    public required string Key { get; set; }
    public required string Name { get; set; }
    public string? Note { get; set; }
}

public class SeedAvailabilitySlotDto
{
    public DayOfWeek DayOfWeek { get; set; }
    public int StartHour { get; set; }
    public int EndHour { get; set; }
    public required string Mode { get; set; }
}
