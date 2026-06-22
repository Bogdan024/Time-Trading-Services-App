using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class CreateMessageDto
{
    [Required]
    [StringLength(2000, MinimumLength = 1)]
    public required string Content { get; set; }
}
