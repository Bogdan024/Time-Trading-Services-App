using API.Entities;
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

    public async Task<IReadOnlyList<TimeTask>> GetTasksAsync(string currentMemberId)
    {
        return await context.TimeTasks
            .Include(x => x.ServiceCategory)
            .Include(x => x.PostedByMember)
            .Include(x => x.AcceptedByMember)
            .Where(x => x.Status == TimeTaskStatus.Open && x.PostedByMemberId != currentMemberId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync();
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
