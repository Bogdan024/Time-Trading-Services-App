using API.DTOs;
using API.Entities;

namespace API.Interfaces;

public interface IReviewRepository
{
    void AddReview(MemberReview review);
    Task<bool> SaveAllAsync();
    Task<TimeTask?> GetTaskForReviewAsync(int taskId);
    Task<MemberReview?> GetReviewForTaskAsync(string reviewerMemberId, int taskId);
    Task<MemberReview?> GetReviewByIdAsync(int reviewId);
    Task<IReadOnlyList<MemberReview>> GetReviewsForMemberAsync(string memberId);
    Task<ReviewSummaryDto> GetReviewSummaryForMemberAsync(string memberId);
    Task<IReadOnlyList<int>> GetReviewedTaskIdsForMemberAsync(string memberId);
}
