using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API.Entities;

public class ModerationReport
{
    public int Id { get; set; }
    public ReportTargetType TargetType { get; set; }
    public int? TargetIntId { get; set; }
    public string? TargetStringId { get; set; }
    public ReportReason Reason { get; set; }

    [MaxLength(1000)]
    public string? Details { get; set; }

    public ReportStatus Status { get; set; } = ReportStatus.Pending;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAtUtc { get; set; }

    [MaxLength(1000)]
    public string? ModeratorNotes { get; set; }

    public string ReporterMemberId { get; set; } = null!;

    [JsonIgnore]
    public Member ReporterMember { get; set; } = null!;

    public string? ReviewedByMemberId { get; set; }

    [JsonIgnore]
    public Member? ReviewedByMember { get; set; }
}
