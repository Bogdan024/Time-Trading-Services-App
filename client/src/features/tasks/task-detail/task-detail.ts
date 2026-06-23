import { DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AccountService } from '../../../core/services/account-service';
import { TaskService } from '../../../core/services/task-service';
import { ToastService } from '../../../core/services/toast-service';
import { TaskApplication, TimeTask } from '../../../types/task';

@Component({
  selector: 'app-task-detail',
  imports: [DatePipe, FormsModule, RouterLink],
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
  protected applications = signal<TaskApplication[]>([]);
  protected applicationMessage = '';
  protected applying = signal(false);
  protected loadingApplications = signal(false);
  protected acceptingApplicationId = signal<number | null>(null);
  protected selectedApplication = signal<TaskApplication | null>(null);

  ngOnInit(): void {
    this.loadTask();
  }

  protected applyForTask() {
    const task = this.task();

    if (!task || !this.canApply(task)) return;

    this.applying.set(true);
    this.taskService.applyForTask(task.id, { message: this.applicationMessage.trim() || undefined }).subscribe({
      next: () => {
        this.toast.success('Application sent');
        this.applicationMessage = '';
        this.applying.set(false);
        this.loadTask();
      },
      error: () => this.applying.set(false),
    });
  }

  protected openAcceptApplicationConfirmation(application: TaskApplication) {
    this.selectedApplication.set(application);
  }

  protected closeAcceptApplicationConfirmation() {
    if (this.acceptingApplicationId()) return;

    this.selectedApplication.set(null);
  }

  protected acceptSelectedApplication() {
    const task = this.task();
    const application = this.selectedApplication();

    if (!task || !application) return;

    this.acceptingApplicationId.set(application.id);
    this.taskService.acceptApplication(task.id, application.id).subscribe({
      next: () => {
        this.toast.success('Application accepted');
        this.acceptingApplicationId.set(null);
        this.selectedApplication.set(null);
        this.loadTask();
      },
      error: () => this.acceptingApplicationId.set(null),
    });
  }

  protected canApply(task: TimeTask) {
    return task.status === 1
      && task.postedByMember.id !== this.accountService.currentUser()?.id
      && !task.hasCurrentUserApplied;
  }

  protected canViewApplications(task: TimeTask) {
    return task.postedByMember.id === this.accountService.currentUser()?.id && task.status === 1;
  }

  protected applicationStatusName(status: number) {
    return {
      1: 'Pending',
      2: 'Accepted',
      3: 'Rejected',
      4: 'Withdrawn',
    }[status] ?? 'Unknown';
  }

  protected canViewConversation(task: TimeTask) {
    const currentUserId = this.accountService.currentUser()?.id;

    return !!task.acceptedByMember
      && (task.postedByMember.id === currentUserId || task.acceptedByMember.id === currentUserId)
      && task.status !== 1;
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
      next: (task) => {
        this.task.set(task);

        if (this.canViewApplications(task)) {
          this.loadApplications(task.id);
        } else {
          this.applications.set([]);
        }
      },
    });
  }

  private loadApplications(taskId: number) {
    this.loadingApplications.set(true);
    this.taskService.getTaskApplications(taskId).subscribe({
      next: (applications) => {
        this.applications.set(applications);
        this.loadingApplications.set(false);
      },
      error: () => this.loadingApplications.set(false),
    });
  }
}


