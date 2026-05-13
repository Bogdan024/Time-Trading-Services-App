import { AsyncPipe, DatePipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { combineLatest, map, Observable } from 'rxjs';
import { TaskService } from '../../../core/services/task-service';
import { TimeTask } from '../../../types/task';

@Component({
  selector: 'app-task-history',
  imports: [AsyncPipe, DatePipe, RouterLink],
  templateUrl: './task-history.html',
  styleUrl: './task-history.css',
})
export class TaskHistory {
  private taskService = inject(TaskService);
  protected tasks$: Observable<TimeTask[]> = combineLatest([
    this.taskService.getMyTasks(),
    this.taskService.getAcceptedTasks(),
  ]).pipe(
    map(([postedTasks, acceptedTasks]) => {
      const tasksById = new Map<number, TimeTask>();

      [...postedTasks, ...acceptedTasks]
        .filter((task) => task.status === 3 || task.status === 4)
        .forEach((task) => tasksById.set(task.id, task));

      return [...tasksById.values()].sort((a, b) => this.sortDate(b).getTime() - this.sortDate(a).getTime());
    }),
  );

  protected statusName(status: number) {
    return {
      3: 'Completed',
      4: 'Cancelled',
    }[status] ?? 'Unknown';
  }

  protected taskRole(task: TimeTask) {
    return task.acceptedByMember ? 'Exchange' : 'Posted request';
  }

  private sortDate(task: TimeTask) {
    return new Date(task.completedAtUtc ?? task.updatedAtUtc ?? task.createdAtUtc);
  }
}
