using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class ResolveReportDto
{
    [StringLength(1000)]
    public string? ModeratorNotes { get; set; }
}
