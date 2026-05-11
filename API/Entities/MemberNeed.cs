using System.Text.Json.Serialization;

namespace API.Entities;

public class MemberNeed
{
    public int Id { get; set; }
    public string MemberId { get; set; } = null!;
    public int ServiceCategoryId { get; set; }
    public string? Note { get; set; }

    [JsonIgnore]
    public Member Member { get; set; } = null!;
    public ServiceCategory ServiceCategory { get; set; } = null!;
}
