import { DatePipe } from '@angular/common';
import { Component, HostListener, effect, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { AccountService } from '../../core/services/account-service';
import { NotificationService } from '../../core/services/notification-service';
import { TaskService } from '../../core/services/task-service';
import { ToastService } from '../../core/services/toast-service';
import { HasRole } from '../../shared/directives/has-role';
import { AppNotification } from '../../types/notification';
import { TimeTransaction } from '../../types/task';
import { LoginCreds } from '../../types/user';

@Component({
  selector: 'app-nav',
  imports: [DatePipe, FormsModule, RouterLink, RouterLinkActive, HasRole],
  templateUrl: './nav.html',
  styleUrl: './nav.css',
})
export class Nav {
  protected accountService = inject(AccountService);
  protected notificationService = inject(NotificationService);
  private taskService = inject(TaskService);
  private router = inject(Router);
  private toast = inject(ToastService);
  protected creds = {} as LoginCreds;
  protected readonly isScrolled = signal(false);
  protected readonly loginLoading = signal(false);
  protected readonly transactions = signal<TimeTransaction[]>([]);

  constructor() {
    effect(() => {
      if (this.accountService.currentUser()) {
        this.loadBalance();
      } else {
        this.transactions.set([]);
      }
    });
  }

  @HostListener('window:scroll')
  onWindowScroll() {
    this.isScrolled.set(window.scrollY > 24);
  }

  protected balance() {
    const userId = this.accountService.currentUser()?.id;

    return this.transactions().reduce((total, transaction) => {
      if (transaction.toMember.id === userId) return total + transaction.hours;
      if (transaction.fromMember.id === userId) return total - transaction.hours;

      return total;
    }, 0);
  }

  login() {
    this.loginLoading.set(true);
    this.accountService.login(this.creds).subscribe({
      next: () => {
        this.creds = {} as LoginCreds;
        this.router.navigateByUrl('/tasks');
        this.toast.success('Signed in');
        this.loginLoading.set(false);
      },
      error: (error) => {
        this.toast.error(error.error ?? 'Login failed');
        this.loginLoading.set(false);
      },
    });
  }

  logout() {
    this.accountService.logout();
    this.router.navigateByUrl('/');
  }

  protected handleSelectUserItem() {
    const activeElement = document.activeElement as HTMLElement | null;

    activeElement?.blur();
  }

  protected openNotification(notification: AppNotification) {
    this.notificationService.markAsRead(notification);

    if (notification.conversationId) {
      this.router.navigate(['/messages'], { queryParams: { conversationId: notification.conversationId } });
      return;
    }

    if (notification.timeTaskId) {
      this.router.navigate(['/tasks', notification.timeTaskId]);
      return;
    }

    if (notification.groupId) {
      this.router.navigate(['/groups', notification.groupId]);
    }
  }

  private loadBalance() {
    this.taskService.getTransactions().subscribe({
      next: (transactions) => this.transactions.set(transactions),
    });
  }
}
