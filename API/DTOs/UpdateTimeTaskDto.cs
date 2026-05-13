using System.ComponentModel.DataAnnotations;
using API.Entities;

namespace API.DTOs;

public class UpdateTimeTaskDto
{
    [Required]
    [StringLength(100, MinimumLength = 5)]
    public required string Title { get; set; }

    [Required]
    [StringLength(2000, MinimumLength = 20)]
    public required string Description { get; set; }

    [Range(1, int.MaxValue)]
    public int ServiceCategoryId { get; set; }

    [Range(1, 24)]
    public int EstimatedHours { get; set; }

    [EnumDataType(typeof(TaskLocationMode))]
    public TaskLocationMode LocationMode { get; set; }

    [StringLength(80)]
    public string? City { get; set; }

    [StringLength(2, MinimumLength = 2)]
    public string? CountryCode { get; set; }

    public DateTime? DueAtUtc { get; set; }
}
