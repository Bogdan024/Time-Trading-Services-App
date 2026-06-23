using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API.Entities;

public class TaskApplication
{
    public int Id { get; set; }
    public int TimeTaskId { get; set; }
    public string ApplicantMemberId { get; set; } = null!;

    [MaxLength(1000)]
    public string? Message { get; set; }

    public TaskApplicationStatus Status { get; set; } = TaskApplicationStatus.Pending;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    [JsonIgnore]
    public TimeTask TimeTask { get; set; } = null!;

    [JsonIgnore]
    public Member ApplicantMember { get; set; } = null!;
}
