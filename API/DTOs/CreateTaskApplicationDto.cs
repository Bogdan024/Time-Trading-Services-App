using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class CreateTaskApplicationDto
{
    [StringLength(1000)]
    public string? Message { get; set; }
}
