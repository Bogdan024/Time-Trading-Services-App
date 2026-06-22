using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class CreateReviewDto
{
    [Range(1, 5)]
    public int Rating { get; set; }

    [StringLength(500)]
    public string? Comment { get; set; }
}
