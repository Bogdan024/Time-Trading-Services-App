using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class TasksController(ITimeTaskRepository taskRepository) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TimeTaskDto>>> GetTasks()
    {
        var tasks = await taskRepository.GetTasksAsync(User.GetMemberId());

        return Ok(tasks.Select(x => x.ToDto()));
    }

    [HttpGet("mine")]
    public async Task<ActionResult<IReadOnlyList<TimeTaskDto>>> GetMyTasks()
    {
        var tasks = await taskRepository.GetTasksForMemberAsync(User.GetMemberId());

        return Ok(tasks.Select(x => x.ToDto()));
    }

    [HttpGet("accepted")]
    public async Task<ActionResult<IReadOnlyList<TimeTaskDto>>> GetAcceptedTasks()
    {
        var tasks = await taskRepository.GetAcceptedTasksForMemberAsync(User.GetMemberId());

        return Ok(tasks.Select(x => x.ToDto()));
    }

    [HttpGet("transactions")]
    public async Task<ActionResult<IReadOnlyList<TimeTransactionDto>>> GetMyTransactions()
    {
        var transactions = await taskRepository.GetTransactionsForMemberAsync(User.GetMemberId());

        return Ok(transactions.Select(x => x.ToDto()));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TimeTaskDto>> GetTask(int id)
    {
        var task = await taskRepository.GetTaskByIdAsync(id);

        if (task is null) return NotFound();

        var memberId = User.GetMemberId();

        if (task.Status == TimeTaskStatus.Cancelled
            && task.PostedByMemberId != memberId
            && task.AcceptedByMemberId != memberId)
        {
            return NotFound();
        }

        return task.ToDto();
    }

    [HttpPost]
    public async Task<ActionResult<TimeTaskDto>> CreateTask(CreateTimeTaskDto createTaskDto)
    {
        if (!await taskRepository.ServiceCategoryExistsAsync(createTaskDto.ServiceCategoryId))
        {
            return BadRequest("Service category does not exist");
        }

        if (IsDueDateInPast(createTaskDto.DueAtUtc))
        {
            return BadRequest("Due date must be in the future");
        }

        var task = new TimeTask
        {
            Title = createTaskDto.Title,
            Description = createTaskDto.Description,
            ServiceCategoryId = createTaskDto.ServiceCategoryId,
            EstimatedHours = createTaskDto.EstimatedHours,
            LocationMode = createTaskDto.LocationMode,
            City = createTaskDto.City,
            CountryCode = createTaskDto.CountryCode,
            DueAtUtc = createTaskDto.DueAtUtc,
            PostedByMemberId = User.GetMemberId()
        };

        taskRepository.Add(task);

        if (!await taskRepository.SaveAllAsync())
        {
            return BadRequest("Failed to create task");
        }

        var createdTask = await taskRepository.GetTaskByIdAsync(task.Id);

        if (createdTask is null) return BadRequest("Failed to load created task");

        return CreatedAtAction(nameof(GetTask), new { id = createdTask.Id }, createdTask.ToDto());
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> UpdateTask(int id, UpdateTimeTaskDto updateTaskDto)
    {
        var task = await taskRepository.GetTaskByIdAsync(id);

        if (task is null) return NotFound();

        if (task.PostedByMemberId != User.GetMemberId()) return Forbid();

        if (task.Status != TimeTaskStatus.Open)
        {
            return BadRequest("Only open tasks can be edited");
        }

        if (!await taskRepository.ServiceCategoryExistsAsync(updateTaskDto.ServiceCategoryId))
        {
            return BadRequest("Service category does not exist");
        }

        if (IsDueDateInPast(updateTaskDto.DueAtUtc))
        {
            return BadRequest("Due date must be in the future");
        }

        task.Title = updateTaskDto.Title;
        task.Description = updateTaskDto.Description;
        task.ServiceCategoryId = updateTaskDto.ServiceCategoryId;
        task.EstimatedHours = updateTaskDto.EstimatedHours;
        task.LocationMode = updateTaskDto.LocationMode;
        task.City = updateTaskDto.City;
        task.CountryCode = updateTaskDto.CountryCode;
        task.DueAtUtc = updateTaskDto.DueAtUtc;
        task.UpdatedAtUtc = DateTime.UtcNow;

        taskRepository.Update(task);

        if (await taskRepository.SaveAllAsync()) return NoContent();

        return BadRequest("Failed to update task");
    }

    [HttpPatch("{id:int}/cancel")]
    public async Task<ActionResult> CancelTask(int id)
    {
        var task = await taskRepository.GetTaskByIdAsync(id);

        if (task is null) return NotFound();

        if (task.PostedByMemberId != User.GetMemberId()) return Forbid();

        if (task.Status != TimeTaskStatus.Open)
        {
            return BadRequest("Only open tasks can be cancelled");
        }

        task.Status = TimeTaskStatus.Cancelled;
        task.UpdatedAtUtc = DateTime.UtcNow;

        taskRepository.Update(task);

        if (await taskRepository.SaveAllAsync()) return NoContent();

        return BadRequest("Failed to cancel task");
    }

    [HttpPatch("{id:int}/accept")]
    public async Task<ActionResult> AcceptTask(int id)
    {
        var task = await taskRepository.GetTaskByIdAsync(id);

        if (task is null) return NotFound();

        var memberId = User.GetMemberId();

        if (task.PostedByMemberId == memberId)
        {
            return BadRequest("You cannot accept your own task");
        }

        if (task.Status != TimeTaskStatus.Open)
        {
            return BadRequest("Only open tasks can be accepted");
        }

        task.AcceptedByMemberId = memberId;
        task.Status = TimeTaskStatus.InProgress;
        task.UpdatedAtUtc = DateTime.UtcNow;

        taskRepository.Update(task);

        if (await taskRepository.SaveAllAsync()) return NoContent();

        return BadRequest("Failed to accept task");
    }

    [HttpPatch("{id:int}/complete")]
    public async Task<ActionResult<TimeTransactionDto>> CompleteTask(int id)
    {
        var task = await taskRepository.GetTaskByIdAsync(id);

        if (task is null) return NotFound();

        if (task.PostedByMemberId != User.GetMemberId()) return Forbid();

        if (task.Status != TimeTaskStatus.InProgress || task.AcceptedByMemberId is null)
        {
            return BadRequest("Task must be accepted before it can be completed");
        }

        if (task.TimeTransaction is not null)
        {
            return BadRequest("Task already has a time transaction");
        }

        task.Status = TimeTaskStatus.Completed;
        task.CompletedAtUtc = DateTime.UtcNow;
        task.UpdatedAtUtc = DateTime.UtcNow;

        var transaction = new TimeTransaction
        {
            TimeTaskId = task.Id,
            FromMemberId = task.PostedByMemberId,
            ToMemberId = task.AcceptedByMemberId,
            Hours = task.EstimatedHours,
            Note = $"Completed task: {task.Title}"
        };

        taskRepository.AddTransaction(transaction);
        taskRepository.Update(task);

        if (!await taskRepository.SaveAllAsync())
        {
            return BadRequest("Failed to complete task");
        }

        var createdTransaction = await taskRepository.GetTransactionForTaskAsync(task.Id);

        if (createdTransaction is null) return BadRequest("Failed to load time transaction");

        return Ok(createdTransaction.ToDto());
    }

    private static bool IsDueDateInPast(DateTime? dueAtUtc)
    {
        return dueAtUtc.HasValue && dueAtUtc.Value <= DateTime.UtcNow;
    }
}
