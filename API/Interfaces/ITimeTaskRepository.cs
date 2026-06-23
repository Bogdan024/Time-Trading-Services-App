using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces;

public interface ITimeTaskRepository
{
    void Add(TimeTask task);
    void AddApplication(TaskApplication application);
    void AddTransaction(TimeTransaction transaction);
    void Update(TimeTask task);
    Task<bool> SaveAllAsync();
    Task<bool> ServiceCategoryExistsAsync(int serviceCategoryId);
    Task<TimeTask?> GetTaskByIdAsync(int id);
    Task<PaginatedResult<TimeTask>> GetTasksAsync(TaskParams taskParams);
    Task<IReadOnlyList<TimeTask>> GetTasksForMemberAsync(string memberId);
    Task<IReadOnlyList<TimeTask>> GetAcceptedTasksForMemberAsync(string memberId);
    Task<IReadOnlyList<TaskApplication>> GetApplicationsForTaskAsync(int taskId);
    Task<TaskApplication?> GetApplicationForTaskAsync(int taskId, int applicationId);
    Task<IReadOnlyDictionary<string, MemberRatingSummaryDto>> GetRatingSummariesForMembersAsync(IEnumerable<string> memberIds);
    Task<TimeTransaction?> GetTransactionForTaskAsync(int taskId);
    Task<IReadOnlyList<TimeTransaction>> GetTransactionsForMemberAsync(string memberId);
    Task<int> GetAvailableTimeCreditsForMemberAsync(string memberId, int? excludedTaskId = null);
}

