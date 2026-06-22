import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { AccountService } from '../../../core/services/account-service';
import { ReviewService } from '../../../core/services/review-service';
import { TaskService } from '../../../core/services/task-service';
import { ToastService } from '../../../core/services/toast-service';
import { ReviewForm } from '../../../shared/review-form/review-form';
import { CreateReview } from '../../../types/review';
import { TimeTask } from '../../../types/task';

@Component({
  selector: 'app-task-history',
  imports: [DatePipe, RouterLink, ReviewForm],
  templateUrl: './task-history.html',
  styleUrl: './task-history.css',
})
export class TaskHistory implements OnInit {
  private accountService = inject(AccountService);
  private reviewService = inject(ReviewService);
  private taskService = inject(TaskService);
  private toast = inject(ToastService);

  protected tasks = signal<TimeTask[]>([]);
  protected reviewedTaskIds = signal<number[]>([]);
  protected reviewTarget = signal<TimeTask | null>(null);
  protected reviewLoading = signal(false);

  ngOnInit() {
    this.loadHistory();
  }

  protected statusName(status: number) {
    return {
      3: 'Completed',
      4: 'Cancelled',
    }[status] ?? 'Unknown';
  }

  protected taskRole(task: TimeTask) {
    return task.acceptedByMember ? 'Exchange' : 'Posted request';
  }

  protected canReview(task: TimeTask) {
    const currentUserId = this.accountService.currentUser()?.id;
    const isParticipant = task.postedByMember.id === currentUserId || task.acceptedByMember?.id === currentUserId;

    return task.status === 3 && !!task.acceptedByMember && isParticipant && !this.reviewedTaskIds().includes(task.id);
  }

  protected hasReviewed(task: TimeTask) {
    return task.status === 3 && this.reviewedTaskIds().includes(task.id);
  }

  protected reviewTargetName(task: TimeTask) {
    const currentUserId = this.accountService.currentUser()?.id;

    if (task.postedByMember.id === currentUserId) {
      return task.acceptedByMember?.displayName ?? 'the helper';
    }

    return task.postedByMember.displayName;
  }

  protected openReview(task: TimeTask) {
    if (!this.canReview(task)) return;

    this.reviewTarget.set(task);
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
        this.reviewedTaskIds.update((ids) => [...ids, task.id]);
        this.reviewTarget.set(null);
        this.reviewLoading.set(false);
        this.toast.success('Review submitted');
      },
      error: () => this.reviewLoading.set(false),
    });
  }

  private loadHistory() {
    forkJoin({
      postedTasks: this.taskService.getMyTasks(),
      acceptedTasks: this.taskService.getAcceptedTasks(),
      reviewedTaskIds: this.reviewService.getReviewedTaskIds(),
    }).subscribe({
      next: ({ postedTasks, acceptedTasks, reviewedTaskIds }) => {
        const tasksById = new Map<number, TimeTask>();

        [...postedTasks, ...acceptedTasks]
          .filter((task) => task.status === 3 || task.status === 4)
          .forEach((task) => tasksById.set(task.id, task));

        this.tasks.set([...tasksById.values()].sort((a, b) => this.sortDate(b).getTime() - this.sortDate(a).getTime()));
        this.reviewedTaskIds.set(reviewedTaskIds);
      },
    });
  }

  private sortDate(task: TimeTask) {
    return new Date(task.completedAtUtc ?? task.updatedAtUtc ?? task.createdAtUtc);
  }
}
