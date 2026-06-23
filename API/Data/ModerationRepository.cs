using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class ModerationRepository(AppDbContext context) : IModerationRepository
{
    public void AddReport(ModerationReport report)
    {
        context.ModerationReports.Add(report);
    }

    public async Task<IReadOnlyList<ModerationReport>> GetPendingReportsAsync()
    {
        return await ReportQuery()
            .Where(x => x.Status == ReportStatus.Pending)
            .OrderBy(x => x.CreatedAtUtc)
            .ToListAsync();
    }

    public async Task<ModerationReport?> GetReportByIdAsync(int id)
    {
        return await ReportQuery().SingleOrDefaultAsync(x => x.Id == id);
    }

    public async Task<bool> ReportTargetExistsAsync(ReportTargetType targetType, int? targetIntId, string? targetStringId)
    {
        return targetType switch
        {
            ReportTargetType.Task => targetIntId.HasValue && await context.TimeTasks.AnyAsync(x => x.Id == targetIntId.Value),
            ReportTargetType.Group => targetIntId.HasValue && await context.CommunityGroups.AnyAsync(x => x.Id == targetIntId.Value),
            ReportTargetType.Member => !string.IsNullOrWhiteSpace(targetStringId) && await context.Members.AnyAsync(x => x.Id == targetStringId),
            ReportTargetType.Message => !string.IsNullOrWhiteSpace(targetStringId) && await context.Messages.AnyAsync(x => x.Id == targetStringId),
            ReportTargetType.Review => targetIntId.HasValue && await context.MemberReviews.AnyAsync(x => x.Id == targetIntId.Value),
            _ => false
        };
    }

    public async Task<bool> HasPendingReportAsync(string reporterMemberId, ReportTargetType targetType, int? targetIntId, string? targetStringId)
    {
        return await context.ModerationReports.AnyAsync(x => x.ReporterMemberId == reporterMemberId
            && x.TargetType == targetType
            && x.TargetIntId == targetIntId
            && x.TargetStringId == targetStringId
            && x.Status == ReportStatus.Pending);
    }

    public async Task<string?> GetTargetTitleAsync(ModerationReport report)
    {
        return report.TargetType switch
        {
            ReportTargetType.Task when report.TargetIntId.HasValue => await context.TimeTasks
                .Where(x => x.Id == report.TargetIntId.Value)
                .Select(x => x.Title)
                .SingleOrDefaultAsync(),
            ReportTargetType.Group when report.TargetIntId.HasValue => await context.CommunityGroups
                .Where(x => x.Id == report.TargetIntId.Value)
                .Select(x => x.Name)
                .SingleOrDefaultAsync(),
            ReportTargetType.Member when !string.IsNullOrWhiteSpace(report.TargetStringId) => await context.Members
                .Where(x => x.Id == report.TargetStringId)
                .Select(x => x.DisplayName)
                .SingleOrDefaultAsync(),
            ReportTargetType.Message when !string.IsNullOrWhiteSpace(report.TargetStringId) => await context.Messages
                .Where(x => x.Id == report.TargetStringId)
                .Select(x => "Message from " + x.SenderMember.DisplayName)
                .SingleOrDefaultAsync(),
            ReportTargetType.Review when report.TargetIntId.HasValue => await context.MemberReviews
                .Where(x => x.Id == report.TargetIntId.Value)
                .Select(x => "Review for " + x.ReviewedMember.DisplayName)
                .SingleOrDefaultAsync(),
            _ => null
        };
    }

    public async Task<string?> GetTargetSummaryAsync(ModerationReport report)
    {
        return report.TargetType switch
        {
            ReportTargetType.Task when report.TargetIntId.HasValue => await context.TimeTasks
                .Where(x => x.Id == report.TargetIntId.Value)
                .Select(x => x.Description)
                .SingleOrDefaultAsync(),
            ReportTargetType.Group when report.TargetIntId.HasValue => await context.CommunityGroups
                .Where(x => x.Id == report.TargetIntId.Value)
                .Select(x => x.Description)
                .SingleOrDefaultAsync(),
            ReportTargetType.Member when !string.IsNullOrWhiteSpace(report.TargetStringId) => await context.Members
                .Where(x => x.Id == report.TargetStringId)
                .Select(x => x.About)
                .SingleOrDefaultAsync(),
            ReportTargetType.Message when !string.IsNullOrWhiteSpace(report.TargetStringId) => await context.Messages
                .Where(x => x.Id == report.TargetStringId)
                .Select(x => x.Content)
                .SingleOrDefaultAsync(),
            ReportTargetType.Review when report.TargetIntId.HasValue => await context.MemberReviews
                .Where(x => x.Id == report.TargetIntId.Value)
                .Select(x => x.Comment)
                .SingleOrDefaultAsync(),
            _ => null
        };
    }

    public async Task<bool> SaveAllAsync()
    {
        return await context.SaveChangesAsync() > 0;
    }

    private IQueryable<ModerationReport> ReportQuery()
    {
        return context.ModerationReports
            .Include(x => x.ReporterMember)
            .Include(x => x.ReviewedByMember);
    }
}

