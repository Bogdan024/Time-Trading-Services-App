using API.Entities;

namespace API.Interfaces;

public interface ITimeTaskRepository
{
    void Add(TimeTask task);
    void AddTransaction(TimeTransaction transaction);
    void Update(TimeTask task);
    Task<bool> SaveAllAsync();
    Task<bool> ServiceCategoryExistsAsync(int serviceCategoryId);
    Task<TimeTask?> GetTaskByIdAsync(int id);
    Task<IReadOnlyList<TimeTask>> GetTasksAsync(string currentMemberId);
    Task<IReadOnlyList<TimeTask>> GetTasksForMemberAsync(string memberId);
    Task<IReadOnlyList<TimeTask>> GetAcceptedTasksForMemberAsync(string memberId);
    Task<TimeTransaction?> GetTransactionForTaskAsync(int taskId);
    Task<IReadOnlyList<TimeTransaction>> GetTransactionsForMemberAsync(string memberId);
}
