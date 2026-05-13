import { DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AccountService } from '../../../core/services/account-service';
import { TaskService } from '../../../core/services/task-service';
import { ToastService } from '../../../core/services/toast-service';
import { TimeTask } from '../../../types/task';

@Component({
  selector: 'app-task-detail',
  imports: [DatePipe, RouterLink],
  templateUrl: './task-detail.html',
  styleUrl: './task-detail.css',
})
export class TaskDetail implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private taskService = inject(TaskService);
  private toast = inject(ToastService);
  protected accountService = inject(AccountService);
  protected task = signal<TimeTask | undefined>(undefined);

  ngOnInit(): void {
    this.loadTask();
  }

  protected acceptTask() {
    const task = this.task();

    if (!task) return;

    this.taskService.acceptTask(task.id).subscribe({
      next: () => {
        this.toast.success('Task accepted');
        this.router.navigateByUrl('/my-tasks/accepted');
      },
    });
  }

  protected canAccept(task: TimeTask) {
    return task.status === 1 && task.postedByMember.id !== this.accountService.currentUser()?.id;
  }

  protected backLink() {
    const from = this.route.snapshot.queryParamMap.get('from');

    if (from === 'my-posted') return '/my-tasks/posted';
    if (from === 'my-accepted') return '/my-tasks/accepted';
    if (from === 'my-history') return '/my-tasks/history';

    return '/tasks';
  }

  protected backLabel() {
    const from = this.route.snapshot.queryParamMap.get('from');

    if (from === 'my-posted') return 'Back to posted tasks';
    if (from === 'my-accepted') return 'Back to accepted tasks';
    if (from === 'my-history') return 'Back to task history';

    return 'Back to tasks';
  }

  protected locationModeName(mode: number) {
    return {
      1: 'In person',
      2: 'Remote',
      3: 'Either',
    }[mode] ?? 'Flexible';
  }

  protected statusName(status: number) {
    return {
      1: 'Open',
      2: 'In progress',
      3: 'Completed',
      4: 'Cancelled',
    }[status] ?? 'Unknown';
  }

  private loadTask() {
    const id = Number(this.route.snapshot.paramMap.get('id'));

    if (!id) {
      this.router.navigateByUrl('/not-found');
      return;
    }

    this.taskService.getTask(id).subscribe({
      next: (task) => this.task.set(task),
    });
  }
}
