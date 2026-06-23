using API.DTOs;
using API.Entities;

namespace API.Extensions;

public static class TaskApplicationExtensions
{
    public static TaskApplicationDto ToDto(this TaskApplication application, double? averageRating = null, int reviewCount = 0)
    {
        return new TaskApplicationDto
        {
            Id = application.Id,
            TimeTaskId = application.TimeTaskId,
            Status = application.Status,
            Message = application.Message,
            CreatedAtUtc = application.CreatedAtUtc,
            UpdatedAtUtc = application.UpdatedAtUtc,
            MatchesTaskCategory = application.ApplicantMember.OfferedSkills.Any(skill => skill.ServiceCategoryId == application.TimeTask.ServiceCategoryId),
            ApplicantAverageRating = averageRating.HasValue ? Math.Round(averageRating.Value, 1) : null,
            ApplicantReviewCount = reviewCount,
            Applicant = application.ApplicantMember.ToTaskMemberDto()
        };
    }
}
