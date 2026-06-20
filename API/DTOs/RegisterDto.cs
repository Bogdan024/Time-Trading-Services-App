using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class RegisterDto
{
    [Required]
    public string DisplayName { get; set; } = "";

    [Required]
    [EmailAddress]
    public string Email { get; set; } = "";

    [Required]
    [MinLength(4)]
    public string Password { get; set; } = "";

    [Required]
    [StringLength(80)]
    public string City { get; set; } = "";

    [Required]
    [MinLength(2)]
    [MaxLength(2)]
    public string CountryCode { get; set; } = "";

    [StringLength(1000)]
    public string? About { get; set; }

    public bool IsProfilePublic { get; set; } = true;
}
