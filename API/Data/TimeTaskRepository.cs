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

    public void AddTransaction(TimeTransaction transaction)
    {
        context.TimeTransactions.Add(transaction);
    }

    public async Task<IReadOnlyList<TimeTask>> GetAcceptedTasksForMemberAsync(string memberId)
    {
        return await context.TimeTasks
            .Include(x => x.ServiceCategory)
            .Include(x => x.PostedByMember)
            .Include(x => x.AcceptedByMember)
            .Where(x => x.AcceptedByMemberId == memberId)
            .OrderByDescending(x => x.UpdatedAtUtc ?? x.CreatedAtUtc)
            .ToListAsync();
    }

    public async Task<TimeTask?> GetTaskByIdAsync(int id)
    {
        return await context.TimeTasks
            .Include(x => x.ServiceCategory)
            .Include(x => x.PostedByMember)
            .Include(x => x.AcceptedByMember)
            .Include(x => x.TimeTransaction)
            .SingleOrDefaultAsync(x => x.Id == id);
    }

    public async Task<PaginatedResult<TimeTask>> GetTasksAsync(TaskParams taskParams)
    {
        var query = context.TimeTasks
            .Include(x => x.ServiceCategory)
            .Include(x => x.PostedByMember)
            .Include(x => x.AcceptedByMember)
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
        return await context.TimeTasks
            .Include(x => x.ServiceCategory)
            .Include(x => x.PostedByMember)
            .Include(x => x.AcceptedByMember)
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
}
