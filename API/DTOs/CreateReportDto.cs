using API.Entities;
using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class CreateReportDto
{
    [Required]
    public ReportTargetType TargetType { get; set; }

    public int? TargetIntId { get; set; }
    public string? TargetStringId { get; set; }

    [Required]
    public ReportReason Reason { get; set; }

    [StringLength(1000)]
    public string? Details { get; set; }
}
