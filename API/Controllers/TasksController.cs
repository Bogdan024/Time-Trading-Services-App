using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class TasksController(
    IUnitOfWork uow,
    INotificationService notificationService) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<PaginatedResult<TimeTaskDto>>> GetTasks([FromQuery] TaskParams taskParams)
    {
        var memberId = User.GetMemberId();
        taskParams.CurrentMemberId = memberId;
        var tasks = await uow.TimeTaskRepository.GetTasksAsync(taskParams);

        return Ok(new PaginatedResult<TimeTaskDto>
        {
            Metadata = tasks.Metadata,
            Items = tasks.Items.Select(x => x.ToDto(memberId)).ToList()
        });
    }

    [HttpGet("mine")]
    public async Task<ActionResult<IReadOnlyList<TimeTaskDto>>> GetMyTasks()
    {
        var memberId = User.GetMemberId();
        var tasks = await uow.TimeTaskRepository.GetTasksForMemberAsync(memberId);

        return Ok(tasks.Select(x => x.ToDto(memberId)));
    }

    [HttpGet("accepted")]
    public async Task<ActionResult<IReadOnlyList<TimeTaskDto>>> GetAcceptedTasks()
    {
        var memberId = User.GetMemberId();
        var tasks = await uow.TimeTaskRepository.GetAcceptedTasksForMemberAsync(memberId);

        return Ok(tasks.Select(x => x.ToDto(memberId)));
    }

    [HttpGet("transactions")]
    public async Task<ActionResult<IReadOnlyList<TimeTransactionDto>>> GetMyTransactions()
    {
        var transactions = await uow.TimeTaskRepository.GetTransactionsForMemberAsync(User.GetMemberId());

        return Ok(transactions.Select(x => x.ToDto()));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TimeTaskDto>> GetTask(int id)
    {
        var task = await uow.TimeTaskRepository.GetTaskByIdAsync(id);

        if (task is null) return NotFound();

        var memberId = User.GetMemberId();

        if (task.Status == TimeTaskStatus.Cancelled
            && task.PostedByMemberId != memberId
            && task.AcceptedByMemberId != memberId)
        {
            return NotFound();
        }

        return task.ToDto(memberId);
    }

    [HttpGet("{id:int}/applications")]
    public async Task<ActionResult<IReadOnlyList<TaskApplicationDto>>> GetTaskApplications(int id)
    {
        var task = await uow.TimeTaskRepository.GetTaskByIdAsync(id);

        if (task is null) return NotFound();
        if (task.PostedByMemberId != User.GetMemberId()) return Forbid();

        var applications = await uow.TimeTaskRepository.GetApplicationsForTaskAsync(id);
        var ratingSummaries = await uow.TimeTaskRepository.GetRatingSummariesForMembersAsync(applications.Select(x => x.ApplicantMemberId));

        return Ok(applications.Select(application =>
        {
            ratingSummaries.TryGetValue(application.ApplicantMemberId, out var ratingSummary);

            return application.ToDto(ratingSummary?.AverageRating, ratingSummary?.ReviewCount ?? 0);
        }));
    }

    [HttpPost]
    public async Task<ActionResult<TimeTaskDto>> CreateTask(CreateTimeTaskDto createTaskDto)
    {
        if (!await uow.TimeTaskRepository.ServiceCategoryExistsAsync(createTaskDto.ServiceCategoryId))
        {
            return BadRequest("Service category does not exist");
        }

        if (IsDueDateInPast(createTaskDto.DueAtUtc))
        {
            return BadRequest("Due date must be in the future");
        }

        var memberId = User.GetMemberId();
        var task = new TimeTask
        {
            Title = createTaskDto.Title,
            Description = createTaskDto.Description,
            ServiceCategoryId = createTaskDto.ServiceCategoryId,
            EstimatedHours = createTaskDto.EstimatedHours,
            LocationMode = createTaskDto.LocationMode,
            City = createTaskDto.City.Trim(),
            CountryCode = createTaskDto.CountryCode.Trim().ToUpperInvariant(),
            FormattedAddress = createTaskDto.FormattedAddress.Trim(),
            PlaceId = string.IsNullOrWhiteSpace(createTaskDto.PlaceId) ? null : createTaskDto.PlaceId.Trim(),
            Latitude = createTaskDto.Latitude,
            Longitude = createTaskDto.Longitude,
            DueAtUtc = createTaskDto.DueAtUtc,
            PostedByMemberId = memberId
        };

        uow.TimeTaskRepository.Add(task);

        if (!await uow.Complete())
        {
            return BadRequest("Failed to create task");
        }

        var createdTask = await uow.TimeTaskRepository.GetTaskByIdAsync(task.Id);

        if (createdTask is null) return BadRequest("Failed to load created task");

        return CreatedAtAction(nameof(GetTask), new { id = createdTask.Id }, createdTask.ToDto(memberId));
    }

    [HttpPost("{id:int}/applications")]
    public async Task<ActionResult<TaskApplicationDto>> ApplyForTask(int id, CreateTaskApplicationDto createApplicationDto)
    {
        var task = await uow.TimeTaskRepository.GetTaskByIdAsync(id);

        if (task is null) return NotFound();

        var memberId = User.GetMemberId();

        if (task.PostedByMemberId == memberId) return BadRequest("You cannot apply to your own task");
        if (task.Status != TimeTaskStatus.Open) return BadRequest("Only open tasks accept applications");

        var message = string.IsNullOrWhiteSpace(createApplicationDto.Message) ? null : createApplicationDto.Message.Trim();
        var existingApplication = task.Applications.FirstOrDefault(x => x.ApplicantMemberId == memberId);

        if (existingApplication is not null && existingApplication.Status != TaskApplicationStatus.Withdrawn)
        {
            return BadRequest("You have already applied to this task");
        }

        if (existingApplication is not null)
        {
            existingApplication.Status = TaskApplicationStatus.Pending;
            existingApplication.Message = message;
            existingApplication.UpdatedAtUtc = DateTime.UtcNow;
        }
        else
        {
            uow.TimeTaskRepository.AddApplication(new TaskApplication
            {
                TimeTaskId = task.Id,
                ApplicantMemberId = memberId,
                Message = message
            });
        }

        var applicationNotification = notificationService.Create(
            task.PostedByMemberId,
            NotificationType.TaskApplicationReceived,
            "New task application",
            $"Someone applied to {task.Title}.",
            timeTaskId: task.Id);

        if (!await uow.Complete()) return BadRequest("Failed to apply for task");

        await notificationService.SendAsync(applicationNotification);

        var applications = await uow.TimeTaskRepository.GetApplicationsForTaskAsync(task.Id);
        var application = applications.SingleOrDefault(x => x.ApplicantMemberId == memberId);

        if (application is null) return BadRequest("Failed to load application");

        var ratingSummaries = await uow.TimeTaskRepository.GetRatingSummariesForMembersAsync([memberId]);
        ratingSummaries.TryGetValue(memberId, out var ratingSummary);

        return Ok(application.ToDto(ratingSummary?.AverageRating, ratingSummary?.ReviewCount ?? 0));
    }

    [HttpPost("{id:int}/applications/{applicationId:int}/accept")]
    public async Task<ActionResult> AcceptApplication(int id, int applicationId)
    {
        var application = await uow.TimeTaskRepository.GetApplicationForTaskAsync(id, applicationId);

        if (application is null) return NotFound();

        var task = application.TimeTask;

        if (task.PostedByMemberId != User.GetMemberId()) return Forbid();
        if (task.Status != TimeTaskStatus.Open) return BadRequest("Only open tasks can accept an application");
        if (application.Status != TaskApplicationStatus.Pending) return BadRequest("Only pending applications can be accepted");

        task.AcceptedByMemberId = application.ApplicantMemberId;
        task.Status = TimeTaskStatus.InProgress;
        task.UpdatedAtUtc = DateTime.UtcNow;
        application.Status = TaskApplicationStatus.Accepted;
        application.UpdatedAtUtc = DateTime.UtcNow;

        foreach (var pendingApplication in task.Applications.Where(x => x.Id != application.Id && x.Status == TaskApplicationStatus.Pending))
        {
            pendingApplication.Status = TaskApplicationStatus.Rejected;
            pendingApplication.UpdatedAtUtc = DateTime.UtcNow;
        }

        var conversation = await uow.MessageRepository.GetOrCreateTaskConversationAsync(task);
        var acceptedNotification = notificationService.Create(
            application.ApplicantMemberId,
            NotificationType.TaskApplicationAccepted,
            "You were chosen for a task",
            $"You were accepted for {task.Title}.",
            timeTaskId: task.Id,
            conversationId: conversation.Id);

        uow.TimeTaskRepository.Update(task);

        if (await uow.Complete())
        {
            await notificationService.SendAsync(acceptedNotification);
            return NoContent();
        }

        return BadRequest("Failed to accept application");
    }

    [HttpPost("{id:int}/applications/{applicationId:int}/withdraw")]
    public async Task<ActionResult> WithdrawApplication(int id, int applicationId)
    {
        var application = await uow.TimeTaskRepository.GetApplicationForTaskAsync(id, applicationId);

        if (application is null) return NotFound();
        if (application.ApplicantMemberId != User.GetMemberId()) return Forbid();
        if (application.TimeTask.Status != TimeTaskStatus.Open) return BadRequest("Only applications for open tasks can be withdrawn");
        if (application.Status != TaskApplicationStatus.Pending) return BadRequest("Only pending applications can be withdrawn");

        application.Status = TaskApplicationStatus.Withdrawn;
        application.UpdatedAtUtc = DateTime.UtcNow;

        if (await uow.Complete()) return NoContent();

        return BadRequest("Failed to withdraw application");
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> UpdateTask(int id, UpdateTimeTaskDto updateTaskDto)
    {
        var task = await uow.TimeTaskRepository.GetTaskByIdAsync(id);

        if (task is null) return NotFound();

        if (task.PostedByMemberId != User.GetMemberId()) return Forbid();

        if (task.Status != TimeTaskStatus.Open)
        {
            return BadRequest("Only open tasks can be edited");
        }

        if (!await uow.TimeTaskRepository.ServiceCategoryExistsAsync(updateTaskDto.ServiceCategoryId))
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
        task.City = updateTaskDto.City.Trim();
        task.CountryCode = updateTaskDto.CountryCode.Trim().ToUpperInvariant();
        task.FormattedAddress = updateTaskDto.FormattedAddress.Trim();
        task.PlaceId = string.IsNullOrWhiteSpace(updateTaskDto.PlaceId) ? null : updateTaskDto.PlaceId.Trim();
        task.Latitude = updateTaskDto.Latitude;
        task.Longitude = updateTaskDto.Longitude;
        task.DueAtUtc = updateTaskDto.DueAtUtc;
        task.UpdatedAtUtc = DateTime.UtcNow;

        uow.TimeTaskRepository.Update(task);

        if (await uow.Complete()) return NoContent();

        return BadRequest("Failed to update task");
    }

    [HttpPatch("{id:int}/cancel")]
    public async Task<ActionResult> CancelTask(int id)
    {
        var task = await uow.TimeTaskRepository.GetTaskByIdAsync(id);

        if (task is null) return NotFound();

        if (task.PostedByMemberId != User.GetMemberId()) return Forbid();

        if (task.Status != TimeTaskStatus.Open)
        {
            return BadRequest("Only open tasks can be cancelled");
        }

        task.Status = TimeTaskStatus.Cancelled;
        task.UpdatedAtUtc = DateTime.UtcNow;

        var cancellationNotifications = new List<Notification>();

        foreach (var pendingApplication in task.Applications.Where(x => x.Status == TaskApplicationStatus.Pending))
        {
            pendingApplication.Status = TaskApplicationStatus.Rejected;
            pendingApplication.UpdatedAtUtc = DateTime.UtcNow;
            cancellationNotifications.Add(notificationService.Create(
                pendingApplication.ApplicantMemberId,
                NotificationType.TaskCancelled,
                "Task was cancelled",
                $"{task.Title} was cancelled by the poster.",
                timeTaskId: task.Id));
        }

        await uow.MessageRepository.CloseTaskConversationAsync(task.Id);
        uow.TimeTaskRepository.Update(task);

        if (await uow.Complete())
        {
            await notificationService.SendAsync(cancellationNotifications);
            return NoContent();
        }

        return BadRequest("Failed to cancel task");
    }

    [HttpPatch("{id:int}/complete")]
    public async Task<ActionResult<TimeTransactionDto>> CompleteTask(int id)
    {
        var task = await uow.TimeTaskRepository.GetTaskByIdAsync(id);

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
        await uow.MessageRepository.CloseTaskConversationAsync(task.Id);

        var completionNotification = notificationService.Create(
            task.AcceptedByMemberId,
            NotificationType.TaskCompleted,
            "Task completed",
            $"{task.Title} was completed. You received {task.EstimatedHours} time credits.",
            timeTaskId: task.Id);

        var transaction = new TimeTransaction
        {
            TimeTaskId = task.Id,
            FromMemberId = task.PostedByMemberId,
            ToMemberId = task.AcceptedByMemberId,
            Hours = task.EstimatedHours,
            Note = $"Completed task: {task.Title}"
        };

        uow.TimeTaskRepository.AddTransaction(transaction);
        uow.TimeTaskRepository.Update(task);

        if (!await uow.Complete())
        {
            return BadRequest("Failed to complete task");
        }

        await notificationService.SendAsync(completionNotification);

        var createdTransaction = await uow.TimeTaskRepository.GetTransactionForTaskAsync(task.Id);

        if (createdTransaction is null) return BadRequest("Failed to load time transaction");

        return Ok(createdTransaction.ToDto());
    }

    private static bool IsDueDateInPast(DateTime? dueAtUtc)
    {
        return dueAtUtc.HasValue && dueAtUtc.Value <= DateTime.UtcNow;
    }
}

