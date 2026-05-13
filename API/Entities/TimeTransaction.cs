using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API.Entities;

public class TimeTransaction
{
    public int Id { get; set; }
    public int TimeTaskId { get; set; }

    [JsonIgnore]
    public TimeTask TimeTask { get; set; } = null!;

    public string FromMemberId { get; set; } = null!;

    [JsonIgnore]
    public Member FromMember { get; set; } = null!;

    public string ToMemberId { get; set; } = null!;

    [JsonIgnore]
    public Member ToMember { get; set; } = null!;

    public int Hours { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    [MaxLength(200)]
    public string? Note { get; set; }
}
