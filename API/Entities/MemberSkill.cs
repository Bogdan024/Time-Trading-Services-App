using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API.Entities;

public class MemberSkill
{
    public int Id { get; set; }
    public string MemberId { get; set; } = null!;
    public int ServiceCategoryId { get; set; }
    [MaxLength(250)]
    public string? Note { get; set; }

    [JsonIgnore]
    public Member Member { get; set; } = null!;
    public ServiceCategory ServiceCategory { get; set; } = null!;
}

