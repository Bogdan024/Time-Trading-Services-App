import { AsyncPipe, DatePipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Observable } from 'rxjs';
import { TaskService } from '../../../core/services/task-service';
import { TimeTask } from '../../../types/task';

@Component({
  selector: 'app-task-list',
  imports: [AsyncPipe, DatePipe, RouterLink],
  templateUrl: './task-list.html',
  styleUrl: './task-list.css',
})
export class TaskList {
  private taskService = inject(TaskService);
  protected tasks$: Observable<TimeTask[]> = this.taskService.getTasks();

  protected locationModeName(mode: number) {
    return {
      1: 'In person',
      2: 'Remote',
      3: 'Either',
    }[mode] ?? 'Flexible';
  }
}
