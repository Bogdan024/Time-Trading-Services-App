using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class RejectGroupDto
{
    [StringLength(1000)]
    public string? Reason { get; set; }
}
