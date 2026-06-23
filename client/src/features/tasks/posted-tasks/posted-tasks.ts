import { AsyncPipe, DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { map, Observable } from 'rxjs';
import { BalanceService } from '../../../core/services/balance-service';
import { ConfirmDialogService } from '../../../core/services/confirm-dialog-service';
import { ReviewService } from '../../../core/services/review-service';
import { TaskService } from '../../../core/services/task-service';
import { ToastService } from '../../../core/services/toast-service';
import { ReviewForm } from '../../../shared/review-form/review-form';
import { CreateReview } from '../../../types/review';
import { TimeTask } from '../../../types/task';

@Component({
  selector: 'app-posted-tasks',
  imports: [AsyncPipe, DatePipe, RouterLink, ReviewForm],
  templateUrl: './posted-tasks.html',
  styleUrl: './posted-tasks.css',
})
export class PostedTasks {
  private balanceService = inject(BalanceService);
  private confirmDialog = inject(ConfirmDialogService);
  private reviewService = inject(ReviewService);
  private taskService = inject(TaskService);
  private toast = inject(ToastService);
  protected tasks$: Observable<TimeTask[]> = this.getVisibleTasks();
  protected reviewTarget = signal<TimeTask | null>(null);
  protected reviewLoading = signal(false);

  protected async cancelTask(task: TimeTask) {
    const confirmed = await this.confirmDialog.confirm(
      `Cancel "${task.title}"?`,
      'Applicants will be notified and this task will move to history.'
    );

    if (!confirmed) return;

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
        this.balanceService.refreshBalance();
        this.tasks$ = this.getVisibleTasks();

        if (task.acceptedByMember) {
          this.reviewTarget.set(task);
        }
      },
    });
  }

  protected closeReview() {
    if (this.reviewLoading()) return;

    this.reviewTarget.set(null);
  }

  protected submitReview(review: CreateReview) {
    const task = this.reviewTarget();

    if (!task) return;

    this.reviewLoading.set(true);
    this.reviewService.createReview(task.id, review).subscribe({
      next: () => {
        this.reviewTarget.set(null);
        this.reviewLoading.set(false);
        this.toast.success('Review submitted');
      },
      error: () => this.reviewLoading.set(false),
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




