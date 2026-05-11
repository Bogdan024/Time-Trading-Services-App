using System.Text.Json.Serialization;

namespace API.Entities;

public class MemberAvailabilitySlot
{
    public int Id { get; set; }
    public string MemberId { get; set; } = null!;
    public DayOfWeek DayOfWeek { get; set; }
    public int StartHour { get; set; }
    public int EndHour { get; set; }
    public AvailabilityMode Mode { get; set; }

    [JsonIgnore]
    public Member Member { get; set; } = null!;
}
