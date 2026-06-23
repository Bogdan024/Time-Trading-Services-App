using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class TimeTaskRepository(AppDbContext context) : ITimeTaskRepository
{
    public void Add(TimeTask task)
    {
        context.TimeTasks.Add(task);
    }

    public void AddApplication(TaskApplication application)
    {
        context.TaskApplications.Add(application);
    }

    public void AddTransaction(TimeTransaction transaction)
    {
        context.TimeTransactions.Add(transaction);
    }

    public async Task<IReadOnlyList<TimeTask>> GetAcceptedTasksForMemberAsync(string memberId)
    {
        return await TaskQuery()
            .Where(x => x.AcceptedByMemberId == memberId)
            .OrderByDescending(x => x.UpdatedAtUtc ?? x.CreatedAtUtc)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<TaskApplication>> GetApplicationsForTaskAsync(int taskId)
    {
        return await context.TaskApplications
            .Include(x => x.TimeTask)
            .Include(x => x.ApplicantMember)
                .ThenInclude(x => x.OfferedSkills)
            .Where(x => x.TimeTaskId == taskId && x.Status == TaskApplicationStatus.Pending)
            .OrderBy(x => x.CreatedAtUtc)
            .ToListAsync();
    }

    public async Task<TaskApplication?> GetApplicationForTaskAsync(int taskId, int applicationId)
    {
        return await context.TaskApplications
            .Include(x => x.ApplicantMember)
                .ThenInclude(x => x.OfferedSkills)
            .Include(x => x.TimeTask)
                .ThenInclude(x => x.Applications)
            .SingleOrDefaultAsync(x => x.Id == applicationId && x.TimeTaskId == taskId);
    }


    public async Task<IReadOnlyDictionary<string, MemberRatingSummaryDto>> GetRatingSummariesForMembersAsync(IEnumerable<string> memberIds)
    {
        var ids = memberIds.Distinct().ToList();

        if (ids.Count == 0) return new Dictionary<string, MemberRatingSummaryDto>();

        return await context.MemberReviews
            .Where(x => ids.Contains(x.ReviewedMemberId))
            .GroupBy(x => x.ReviewedMemberId)
            .Select(group => new MemberRatingSummaryDto
            {
                MemberId = group.Key,
                AverageRating = group.Average(review => review.Rating),
                ReviewCount = group.Count()
            })
            .ToDictionaryAsync(x => x.MemberId);
    }
    public async Task<TimeTask?> GetTaskByIdAsync(int id)
    {
        return await TaskQuery()
            .SingleOrDefaultAsync(x => x.Id == id);
    }

    public async Task<PaginatedResult<TimeTask>> GetTasksAsync(TaskParams taskParams)
    {
        var query = TaskQuery()
            .Where(x => x.Status == TimeTaskStatus.Open && x.PostedByMemberId != taskParams.CurrentMemberId)
            .AsQueryable();

        if (taskParams.ServiceCategoryId.HasValue)
        {
            query = query.Where(x => x.ServiceCategoryId == taskParams.ServiceCategoryId.Value);
        }

        if (taskParams.LocationMode.HasValue)
        {
            query = query.Where(x => x.LocationMode == taskParams.LocationMode.Value);
        }

        if (!string.IsNullOrWhiteSpace(taskParams.City))
        {
            var city = taskParams.City.Trim().ToLower();
            query = query.Where(x => x.City != null && x.City.ToLower() == city);
        }

        if (!string.IsNullOrWhiteSpace(taskParams.CountryCode))
        {
            var countryCode = taskParams.CountryCode.Trim().ToUpper();
            query = query.Where(x => x.CountryCode != null && x.CountryCode.ToUpper() == countryCode);
        }

        if (taskParams.MinCredits.HasValue)
        {
            query = query.Where(x => x.EstimatedHours >= taskParams.MinCredits.Value);
        }

        if (taskParams.MaxCredits.HasValue)
        {
            query = query.Where(x => x.EstimatedHours <= taskParams.MaxCredits.Value);
        }

        if (taskParams.DueSoon)
        {
            var dueSoonCutoff = DateTime.UtcNow.AddDays(7);
            query = query.Where(x => x.DueAtUtc.HasValue && x.DueAtUtc <= dueSoonCutoff);
        }

        if (taskParams.MinPosterRating.HasValue)
        {
            query = query.Where(task => context.MemberReviews
                .Where(review => review.ReviewedMemberId == task.PostedByMemberId)
                .Average(review => (double?)review.Rating) >= taskParams.MinPosterRating.Value);
        }

        query = taskParams.OrderBy switch
        {
            "dueSoon" => query.OrderBy(x => x.DueAtUtc ?? DateTime.MaxValue),
            "credits" => query.OrderByDescending(x => x.EstimatedHours),
            _ => query.OrderByDescending(x => x.CreatedAtUtc)
        };

        return await PaginationHelper.CreateAsync(query, taskParams.PageNumber, taskParams.PageSize);
    }

    public async Task<IReadOnlyList<TimeTask>> GetTasksForMemberAsync(string memberId)
    {
        return await TaskQuery()
            .Where(x => x.PostedByMemberId == memberId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync();
    }

    public async Task<TimeTransaction?> GetTransactionForTaskAsync(int taskId)
    {
        return await context.TimeTransactions
            .Include(x => x.TimeTask)
            .Include(x => x.FromMember)
            .Include(x => x.ToMember)
            .SingleOrDefaultAsync(x => x.TimeTaskId == taskId);
    }

    public async Task<IReadOnlyList<TimeTransaction>> GetTransactionsForMemberAsync(string memberId)
    {
        return await context.TimeTransactions
            .Include(x => x.TimeTask)
            .Include(x => x.FromMember)
            .Include(x => x.ToMember)
            .Where(x => x.FromMemberId == memberId || x.ToMemberId == memberId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync();
    }

    public async Task<int> GetAvailableTimeCreditsForMemberAsync(string memberId, int? excludedTaskId = null)
    {
        var earned = await context.TimeTransactions
            .Where(x => x.ToMemberId == memberId)
            .SumAsync(x => (int?)x.Hours) ?? 0;

        var spent = await context.TimeTransactions
            .Where(x => x.FromMemberId == memberId)
            .SumAsync(x => (int?)x.Hours) ?? 0;

        var reservedTasks = context.TimeTasks
            .Where(x => x.PostedByMemberId == memberId)
            .Where(x => x.Status == TimeTaskStatus.Open || x.Status == TimeTaskStatus.InProgress);

        if (excludedTaskId.HasValue)
        {
            reservedTasks = reservedTasks.Where(x => x.Id != excludedTaskId.Value);
        }

        var reserved = await reservedTasks.SumAsync(x => (int?)x.EstimatedHours) ?? 0;

        return earned - spent - reserved;
    }

    public async Task<bool> SaveAllAsync()
    {
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<bool> ServiceCategoryExistsAsync(int serviceCategoryId)
    {
        return await context.ServiceCategories.AnyAsync(x => x.Id == serviceCategoryId);
    }

    public void Update(TimeTask task)
    {
        context.Entry(task).State = EntityState.Modified;
    }

    private IQueryable<TimeTask> TaskQuery()
    {
        return context.TimeTasks
            .AsSplitQuery()
            .Include(x => x.ServiceCategory)
            .Include(x => x.PostedByMember)
            .Include(x => x.AcceptedByMember)
            .Include(x => x.TimeTransaction)
            .Include(x => x.Applications)
                .ThenInclude(x => x.ApplicantMember);
    }
}

