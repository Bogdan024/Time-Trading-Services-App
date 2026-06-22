using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class ReviewRepository(AppDbContext context) : IReviewRepository
{
    public void AddReview(MemberReview review)
    {
        context.MemberReviews.Add(review);
    }

    public async Task<TimeTask?> GetTaskForReviewAsync(int taskId)
    {
        return await context.TimeTasks
            .Include(x => x.PostedByMember)
            .Include(x => x.AcceptedByMember)
            .SingleOrDefaultAsync(x => x.Id == taskId);
    }

    public async Task<MemberReview?> GetReviewForTaskAsync(string reviewerMemberId, int taskId)
    {
        return await context.MemberReviews
            .SingleOrDefaultAsync(x => x.ReviewerMemberId == reviewerMemberId && x.TimeTaskId == taskId);
    }

    public async Task<MemberReview?> GetReviewByIdAsync(int reviewId)
    {
        return await context.MemberReviews
            .Include(x => x.TimeTask)
            .Include(x => x.ReviewerMember)
            .Include(x => x.ReviewedMember)
            .SingleOrDefaultAsync(x => x.Id == reviewId);
    }

    public async Task<IReadOnlyList<MemberReview>> GetReviewsForMemberAsync(string memberId)
    {
        return await context.MemberReviews
            .Include(x => x.TimeTask)
            .Include(x => x.ReviewerMember)
            .Include(x => x.ReviewedMember)
            .Where(x => x.ReviewedMemberId == memberId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync();
    }

    public async Task<ReviewSummaryDto> GetReviewSummaryForMemberAsync(string memberId)
    {
        var query = context.MemberReviews.Where(x => x.ReviewedMemberId == memberId);
        var reviewCount = await query.CountAsync();

        return new ReviewSummaryDto
        {
            ReviewCount = reviewCount,
            AverageRating = reviewCount == 0 ? 0 : Math.Round(await query.AverageAsync(x => x.Rating), 1)
        };
    }

    public async Task<IReadOnlyList<int>> GetReviewedTaskIdsForMemberAsync(string memberId)
    {
        return await context.MemberReviews
            .Where(x => x.ReviewerMemberId == memberId)
            .Select(x => x.TimeTaskId)
            .ToListAsync();
    }

    public async Task<bool> SaveAllAsync()
    {
        return await context.SaveChangesAsync() > 0;
    }
}
