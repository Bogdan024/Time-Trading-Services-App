using API.DTOs;
using API.Entities;

namespace API.Extensions;

public static class MemberReviewExtensions
{
    public static MemberReviewDto ToDto(this MemberReview review)
    {
        return new MemberReviewDto
        {
            Id = review.Id,
            TimeTaskId = review.TimeTaskId,
            TaskTitle = review.TimeTask.Title,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAtUtc = review.CreatedAtUtc,
            Reviewer = review.ReviewerMember.ToTaskMemberDto(),
            ReviewedMember = review.ReviewedMember.ToTaskMemberDto()
        };
    }
}
