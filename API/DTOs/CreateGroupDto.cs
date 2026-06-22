using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class CreateGroupDto
{
    [Required]
    [StringLength(80, MinimumLength = 3)]
    public required string Name { get; set; }

    [Required]
    [StringLength(1000, MinimumLength = 10)]
    public required string Description { get; set; }

    [StringLength(60)]
    public string? Theme { get; set; }

    [StringLength(80)]
    public string? City { get; set; }

    [StringLength(2, MinimumLength = 2)]
    public string? CountryCode { get; set; }

    public bool IsPublic { get; set; } = true;
}
