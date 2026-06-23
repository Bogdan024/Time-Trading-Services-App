using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class ReportsController(IModerationRepository moderationRepository) : BaseApiController
{
    [HttpPost]
    public async Task<ActionResult<ModerationReportDto>> CreateReport(CreateReportDto createReportDto)
    {
        var memberId = User.GetMemberId();
        var targetStringId = string.IsNullOrWhiteSpace(createReportDto.TargetStringId) ? null : createReportDto.TargetStringId.Trim();

        if (!await moderationRepository.ReportTargetExistsAsync(createReportDto.TargetType, createReportDto.TargetIntId, targetStringId))
        {
            return BadRequest("Report target does not exist");
        }

        if (await moderationRepository.HasPendingReportAsync(memberId, createReportDto.TargetType, createReportDto.TargetIntId, targetStringId))
        {
            return BadRequest("You already have a pending report for this item");
        }

        var report = new ModerationReport
        {
            ReporterMemberId = memberId,
            TargetType = createReportDto.TargetType,
            TargetIntId = createReportDto.TargetIntId,
            TargetStringId = targetStringId,
            Reason = createReportDto.Reason,
            Details = string.IsNullOrWhiteSpace(createReportDto.Details) ? null : createReportDto.Details.Trim()
        };

        moderationRepository.AddReport(report);

        if (!await moderationRepository.SaveAllAsync()) return BadRequest("Failed to create report");

        var createdReport = await moderationRepository.GetReportByIdAsync(report.Id);

        if (createdReport is null) return BadRequest("Failed to load report");

        return Ok(await ToReportDto(createdReport));
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
            TargetTitle = await moderationRepository.GetTargetTitleAsync(report),
            TargetSummary = await moderationRepository.GetTargetSummaryAsync(report)
        };
    }
}
