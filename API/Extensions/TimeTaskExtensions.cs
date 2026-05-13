using API.DTOs;
using API.Entities;

namespace API.Extensions;

public static class TimeTaskExtensions
{
    public static TimeTaskDto ToDto(this TimeTask task)
    {
        return new TimeTaskDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            EstimatedHours = task.EstimatedHours,
            LocationMode = task.LocationMode,
            City = task.City,
            CountryCode = task.CountryCode,
            CreatedAtUtc = task.CreatedAtUtc,
            UpdatedAtUtc = task.UpdatedAtUtc,
            DueAtUtc = task.DueAtUtc,
            CompletedAtUtc = task.CompletedAtUtc,
            Status = task.Status,
            ServiceCategory = new ServiceCategoryDto
            {
                Id = task.ServiceCategory.Id,
                Key = task.ServiceCategory.Key,
                Name = task.ServiceCategory.Name
            },
            PostedByMember = task.PostedByMember.ToTaskMemberDto(),
            AcceptedByMember = task.AcceptedByMember?.ToTaskMemberDto()
        };
    }
}
