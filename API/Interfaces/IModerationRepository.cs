using API.Entities;

namespace API.Interfaces;

public interface IModerationRepository
{
    void AddReport(ModerationReport report);
    Task<IReadOnlyList<ModerationReport>> GetPendingReportsAsync();
    Task<ModerationReport?> GetReportByIdAsync(int id);
    Task<bool> ReportTargetExistsAsync(ReportTargetType targetType, int? targetIntId, string? targetStringId);
    Task<bool> HasPendingReportAsync(string reporterMemberId, ReportTargetType targetType, int? targetIntId, string? targetStringId);
    Task<string?> GetTargetTitleAsync(ModerationReport report);
    Task<string?> GetTargetSummaryAsync(ModerationReport report);
    Task<bool> SaveAllAsync();
}
