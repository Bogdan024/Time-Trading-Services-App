using API.DTOs;
using API.Entities;

namespace API.Extensions;

public static class TimeTransactionExtensions
{
    public static TimeTransactionDto ToDto(this TimeTransaction transaction)
    {
        return new TimeTransactionDto
        {
            Id = transaction.Id,
            TimeTaskId = transaction.TimeTaskId,
            TaskTitle = transaction.TimeTask.Title,
            Hours = transaction.Hours,
            CreatedAtUtc = transaction.CreatedAtUtc,
            Note = transaction.Note,
            FromMember = transaction.FromMember.ToTaskMemberDto(),
            ToMember = transaction.ToMember.ToTaskMemberDto()
        };
    }
}
