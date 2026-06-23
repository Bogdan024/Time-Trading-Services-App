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

    [Range(1, 10)]
    public int EstimatedHours { get; set; }

    [EnumDataType(typeof(TaskLocationMode))]
    public TaskLocationMode LocationMode { get; set; }

    [Required]
    [StringLength(80)]
    public required string City { get; set; }

    [Required]
    [StringLength(2, MinimumLength = 2)]
    public required string CountryCode { get; set; }

    [Required]
    [StringLength(250)]
    public required string FormattedAddress { get; set; }

    [StringLength(150)]
    public string? PlaceId { get; set; }

    [Range(-90, 90)]
    public double Latitude { get; set; }

    [Range(-180, 180)]
    public double Longitude { get; set; }

    public DateTime? DueAtUtc { get; set; }
}

