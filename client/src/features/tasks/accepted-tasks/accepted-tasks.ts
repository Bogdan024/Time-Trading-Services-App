import { AsyncPipe, DatePipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { map, Observable } from 'rxjs';
import { TaskService } from '../../../core/services/task-service';
import { TimeTask } from '../../../types/task';

@Component({
  selector: 'app-accepted-tasks',
  imports: [AsyncPipe, DatePipe, RouterLink],
  templateUrl: './accepted-tasks.html',
  styleUrl: './accepted-tasks.css',
})
export class AcceptedTasks {
  private taskService = inject(TaskService);
  protected tasks$: Observable<TimeTask[]> = this.getVisibleTasks();

  protected statusName(status: number) {
    return {
      1: 'Open',
      2: 'In progress',
      3: 'Completed',
      4: 'Cancelled',
    }[status] ?? 'Unknown';
  }

  private getVisibleTasks() {
    return this.taskService
      .getAcceptedTasks()
      .pipe(map((tasks) => tasks.filter((task) => task.status !== 3 && task.status !== 4)));
  }
}
