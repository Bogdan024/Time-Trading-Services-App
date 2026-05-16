using System.ComponentModel.DataAnnotations;
using API.Entities;

namespace API.DTOs;

public class MemberAvailabilitySlotEditDto
{
    [EnumDataType(typeof(DayOfWeek))]
    public DayOfWeek DayOfWeek { get; set; }

    [Range(0, 23)]
    public int StartHour { get; set; }

    [Range(1, 24)]
    public int EndHour { get; set; }

    [EnumDataType(typeof(AvailabilityMode))]
    public AvailabilityMode Mode { get; set; }
}
