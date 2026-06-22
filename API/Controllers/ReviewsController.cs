using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class ReviewsController(IReviewRepository reviewRepository) : BaseApiController
{
    [HttpPost("tasks/{taskId:int}")]
    public async Task<ActionResult<MemberReviewDto>> CreateReview(int taskId, CreateReviewDto createReviewDto)
    {
        var reviewerMemberId = User.GetMemberId();
        var task = await reviewRepository.GetTaskForReviewAsync(taskId);

        if (task is null) return NotFound("Task not found");

        if (task.Status != TimeTaskStatus.Completed || task.AcceptedByMemberId is null)
        {
            return BadRequest("Only completed exchanges can be reviewed");
        }

        var isPoster = task.PostedByMemberId == reviewerMemberId;
        var isHelper = task.AcceptedByMemberId == reviewerMemberId;

        if (!isPoster && !isHelper) return Forbid();

        var reviewedMemberId = isPoster ? task.AcceptedByMemberId : task.PostedByMemberId;

        if (reviewedMemberId == reviewerMemberId)
        {
            return BadRequest("You cannot review yourself");
        }

        if (await reviewRepository.GetReviewForTaskAsync(reviewerMemberId, taskId) is not null)
        {
            return BadRequest("You have already reviewed this exchange");
        }

        var review = new MemberReview
        {
            TimeTaskId = taskId,
            ReviewerMemberId = reviewerMemberId,
            ReviewedMemberId = reviewedMemberId,
            Rating = createReviewDto.Rating,
            Comment = string.IsNullOrWhiteSpace(createReviewDto.Comment) ? null : createReviewDto.Comment.Trim()
        };

        reviewRepository.AddReview(review);

        if (!await reviewRepository.SaveAllAsync()) return BadRequest("Failed to add review");

        var createdReview = await reviewRepository.GetReviewByIdAsync(review.Id);

        if (createdReview is null) return BadRequest("Failed to load created review");

        return Ok(createdReview.ToDto());
    }

    [HttpGet("members/{memberId}")]
    public async Task<ActionResult<IReadOnlyList<MemberReviewDto>>> GetMemberReviews(string memberId)
    {
        var reviews = await reviewRepository.GetReviewsForMemberAsync(memberId);

        return Ok(reviews.Select(x => x.ToDto()));
    }

    [HttpGet("members/{memberId}/summary")]
    public async Task<ActionResult<ReviewSummaryDto>> GetMemberReviewSummary(string memberId)
    {
        return Ok(await reviewRepository.GetReviewSummaryForMemberAsync(memberId));
    }

    [HttpGet("mine/task-ids")]
    public async Task<ActionResult<IReadOnlyList<int>>> GetReviewedTaskIds()
    {
        return Ok(await reviewRepository.GetReviewedTaskIdsForMemberAsync(User.GetMemberId()));
    }
}
