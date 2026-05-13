import { AsyncPipe, DatePipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { map, Observable } from 'rxjs';
import { TaskService } from '../../../core/services/task-service';
import { ToastService } from '../../../core/services/toast-service';
import { TimeTask } from '../../../types/task';

@Component({
  selector: 'app-posted-tasks',
  imports: [AsyncPipe, DatePipe, RouterLink],
  templateUrl: './posted-tasks.html',
  styleUrl: './posted-tasks.css',
})
export class PostedTasks {
  private taskService = inject(TaskService);
  private toast = inject(ToastService);
  protected tasks$: Observable<TimeTask[]> = this.getVisibleTasks();

  protected cancelTask(task: TimeTask) {
    if (!confirm(`Are you sure you want to cancel "${task.title}"?`)) return;

    this.taskService.cancelTask(task.id).subscribe({
      next: () => {
        this.toast.success('Task cancelled');
        this.tasks$ = this.getVisibleTasks();
      },
    });
  }

  protected completeTask(task: TimeTask) {
    this.taskService.completeTask(task.id).subscribe({
      next: () => {
        this.toast.success('Task completed');
        this.tasks$ = this.getVisibleTasks();
      },
    });
  }

  protected statusName(status: number) {
    return {
      1: 'Open',
      2: 'In progress',
      3: 'Completed',
      4: 'Cancelled',
    }[status] ?? 'Unknown';
  }

  private getVisibleTasks() {
    return this.taskService.getMyTasks().pipe(map((tasks) => tasks.filter((task) => task.status !== 3 && task.status !== 4)));
  }
}
