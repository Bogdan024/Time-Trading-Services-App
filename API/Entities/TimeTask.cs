using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API.Entities;

public class TimeTask
{
    public int Id { get; set; }

    [MaxLength(100)]
    public required string Title { get; set; }

    [MaxLength(2000)]
    public required string Description { get; set; }

    public int EstimatedHours { get; set; }
    public TaskLocationMode LocationMode { get; set; }

    [MaxLength(80)]
    public string? City { get; set; }

    [MaxLength(2)]
    public string? CountryCode { get; set; }

    [MaxLength(250)]
    public string? FormattedAddress { get; set; }

    [MaxLength(150)]
    public string? PlaceId { get; set; }

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
    public DateTime? DueAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public TimeTaskStatus Status { get; set; } = TimeTaskStatus.Open;

    public int ServiceCategoryId { get; set; }

    [JsonIgnore]
    public ServiceCategory ServiceCategory { get; set; } = null!;

    public string PostedByMemberId { get; set; } = null!;

    [JsonIgnore]
    public Member PostedByMember { get; set; } = null!;

    public string? AcceptedByMemberId { get; set; }

    [JsonIgnore]
    public Member? AcceptedByMember { get; set; }

    [JsonIgnore]
    public TimeTransaction? TimeTransaction { get; set; }

    [JsonIgnore]
    public List<TaskApplication> Applications { get; set; } = [];
}

