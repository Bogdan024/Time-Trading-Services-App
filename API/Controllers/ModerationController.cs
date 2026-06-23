using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize(Policy = "ModeratePlatformRole")]
public class ModerationController(IUnitOfWork uow, INotificationService notificationService) : BaseApiController
{
    [Authorize(Policy = "RequireAdminRole")]
    [HttpGet("groups/pending")]
    public async Task<ActionResult<IReadOnlyList<PendingGroupDto>>> GetPendingGroups()
    {
        var groups = await uow.GroupRepository.GetPendingGroupsAsync();

        return Ok(groups.Select(ToPendingGroupDto));
    }

    [Authorize(Policy = "RequireAdminRole")]
    [HttpPost("groups/{id:int}/approve")]
    public async Task<ActionResult<GroupDto>> ApproveGroup(int id)
    {
        var moderatorId = User.GetMemberId();
        var group = await uow.GroupRepository.GetGroupForModerationAsync(id);

        if (group is null) return NotFound();
        if (group.ModerationStatus == ModerationStatus.Approved) return BadRequest("Group is already approved");

        group.ModerationStatus = ModerationStatus.Approved;
        group.ReviewedByMemberId = moderatorId;
        group.ReviewedAtUtc = DateTime.UtcNow;
        group.RejectionReason = null;

        await uow.MessageRepository.GetOrCreateGroupConversationAsync(group);

        if (!await uow.Complete()) return BadRequest("Failed to approve group");

        return Ok(group.ToDto(moderatorId));
    }

    [Authorize(Policy = "RequireAdminRole")]
    [HttpPost("groups/{id:int}/reject")]
    public async Task<ActionResult<GroupDto>> RejectGroup(int id, RejectGroupDto rejectGroupDto)
    {
        var moderatorId = User.GetMemberId();
        var group = await uow.GroupRepository.GetGroupForModerationAsync(id);

        if (group is null) return NotFound();
        if (group.ModerationStatus == ModerationStatus.Approved) return BadRequest("Approved groups cannot be rejected here");

        group.ModerationStatus = ModerationStatus.Rejected;
        group.ReviewedByMemberId = moderatorId;
        group.ReviewedAtUtc = DateTime.UtcNow;
        group.RejectionReason = string.IsNullOrWhiteSpace(rejectGroupDto.Reason) ? null : rejectGroupDto.Reason.Trim();

        if (!await uow.Complete()) return BadRequest("Failed to reject group");

        return Ok(group.ToDto(moderatorId));
    }

    [HttpGet("reports")]
    public async Task<ActionResult<IReadOnlyList<ModerationReportDto>>> GetPendingReports()
    {
        var reports = await uow.ModerationRepository.GetPendingReportsAsync();
        var reportDtos = new List<ModerationReportDto>();

        foreach (var report in reports)
        {
            reportDtos.Add(await ToReportDto(report));
        }

        return Ok(reportDtos);
    }

    [HttpPost("reports/{id:int}/dismiss")]
    public async Task<ActionResult<ModerationReportDto>> DismissReport(int id, ResolveReportDto resolveReportDto)
    {
        return await ResolveReport(id, ReportStatus.Dismissed, resolveReportDto);
    }

    [HttpPost("reports/{id:int}/action")]
    public async Task<ActionResult<ModerationReportDto>> ActionReport(int id, ResolveReportDto resolveReportDto)
    {
        return await ResolveReport(id, ReportStatus.Actioned, resolveReportDto);
    }

    [HttpPost("reports/{id:int}/tasks/cancel")]
    public async Task<ActionResult<ModerationReportDto>> CancelReportedTask(int id, ResolveReportDto resolveReportDto)
    {
        var report = await uow.ModerationRepository.GetReportByIdAsync(id);

        if (report is null) return NotFound("Report not found");
        if (report.Status != ReportStatus.Pending) return BadRequest("Only pending reports can be actioned");
        if (report.TargetType != ReportTargetType.Task || report.TargetIntId is null)
        {
            return BadRequest("This report is not connected to a task");
        }

        var task = await uow.TimeTaskRepository.GetTaskByIdAsync(report.TargetIntId.Value);

        if (task is null) return NotFound("Task not found");
        if (task.Status == TimeTaskStatus.Completed) return BadRequest("Completed tasks cannot be cancelled by moderation");
        if (task.Status == TimeTaskStatus.Cancelled) return BadRequest("Task is already cancelled");

        task.Status = TimeTaskStatus.Cancelled;
        task.UpdatedAtUtc = DateTime.UtcNow;

        var notifications = new List<Notification>
        {
            notificationService.Create(
                task.PostedByMemberId,
                NotificationType.TaskCancelled,
                "Task cancelled by moderation",
                $"{task.Title} was cancelled after a moderation review.",
                timeTaskId: task.Id)
        };

        if (!string.IsNullOrWhiteSpace(task.AcceptedByMemberId))
        {
            notifications.Add(notificationService.Create(
                task.AcceptedByMemberId,
                NotificationType.TaskCancelled,
                "Task cancelled by moderation",
                $"{task.Title} was cancelled after a moderation review.",
                timeTaskId: task.Id));
        }

        foreach (var pendingApplication in task.Applications.Where(x => x.Status == TaskApplicationStatus.Pending))
        {
            pendingApplication.Status = TaskApplicationStatus.Rejected;
            pendingApplication.UpdatedAtUtc = DateTime.UtcNow;
            notifications.Add(notificationService.Create(
                pendingApplication.ApplicantMemberId,
                NotificationType.TaskCancelled,
                "Task cancelled by moderation",
                $"{task.Title} was cancelled after a moderation review."));
        }

        await uow.MessageRepository.CloseTaskConversationAsync(task.Id);
        uow.TimeTaskRepository.Update(task);

        report.Status = ReportStatus.Actioned;
        report.ReviewedByMemberId = User.GetMemberId();
        report.ReviewedAtUtc = DateTime.UtcNow;
        report.ModeratorNotes = string.IsNullOrWhiteSpace(resolveReportDto.ModeratorNotes)
            ? "Task cancelled by moderation."
            : resolveReportDto.ModeratorNotes.Trim();

        if (!await uow.Complete()) return BadRequest("Failed to cancel reported task");

        await notificationService.SendAsync(notifications);

        return Ok(await ToReportDto(report));
    }

    private async Task<ActionResult<ModerationReportDto>> ResolveReport(int id, ReportStatus status, ResolveReportDto resolveReportDto)
    {
        var report = await uow.ModerationRepository.GetReportByIdAsync(id);

        if (report is null) return NotFound();
        if (report.Status != ReportStatus.Pending) return BadRequest("Only pending reports can be reviewed");

        report.Status = status;
        report.ReviewedByMemberId = User.GetMemberId();
        report.ReviewedAtUtc = DateTime.UtcNow;
        report.ModeratorNotes = string.IsNullOrWhiteSpace(resolveReportDto.ModeratorNotes) ? null : resolveReportDto.ModeratorNotes.Trim();

        if (!await uow.Complete()) return BadRequest("Failed to resolve report");

        return Ok(await ToReportDto(report));
    }

    private static PendingGroupDto ToPendingGroupDto(CommunityGroup group)
    {
        return new PendingGroupDto
        {
            Id = group.Id,
            Name = group.Name,
            Description = group.Description,
            Theme = group.Theme,
            City = group.City,
            CountryCode = group.CountryCode,
            ModerationStatus = group.ModerationStatus.ToString(),
            CreatedAtUtc = group.CreatedAtUtc,
            Owner = group.OwnerMember.ToTaskMemberDto()
        };
    }

    private async Task<ModerationReportDto> ToReportDto(ModerationReport report)
    {
        return new ModerationReportDto
        {
            Id = report.Id,
            TargetType = report.TargetType.ToString(),
            TargetIntId = report.TargetIntId,
            TargetStringId = report.TargetStringId,
            Reason = report.Reason.ToString(),
            Details = report.Details,
            Status = report.Status.ToString(),
            CreatedAtUtc = report.CreatedAtUtc,
            ReviewedAtUtc = report.ReviewedAtUtc,
            ModeratorNotes = report.ModeratorNotes,
            Reporter = report.ReporterMember.ToTaskMemberDto(),
            ReviewedBy = report.ReviewedByMember?.ToTaskMemberDto(),
            TargetTitle = await uow.ModerationRepository.GetTargetTitleAsync(report),
            TargetSummary = await uow.ModerationRepository.GetTargetSummaryAsync(report)
        };
    }
}

