using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class MemberServiceCategoryEditDto
{
    [Range(1, int.MaxValue)]
    public int ServiceCategoryId { get; set; }

    [StringLength(250)]
    public string? Note { get; set; }
}
