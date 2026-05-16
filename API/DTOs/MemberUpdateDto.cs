using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class MemberUpdateDto
{
    [StringLength(80, MinimumLength = 2)]
    public string? DisplayName { get; set; }

    [StringLength(1000)]
    public string? About { get; set; }

    [StringLength(80)]
    public string? City { get; set; }

    [StringLength(2, MinimumLength = 2)]
    public string? CountryCode { get; set; }

    public bool? IsProfilePublic { get; set; }
}
