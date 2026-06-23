import { Component, HostListener, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { AccountService } from '../../core/services/account-service';
import { NotificationService } from '../../core/services/notification-service';
import { ToastService } from '../../core/services/toast-service';
import { HasRole } from '../../shared/directives/has-role';
import { AppNotification } from '../../types/notification';
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
  private router = inject(Router);
  private toast = inject(ToastService);
  protected creds = {} as LoginCreds;
  protected readonly isScrolled = signal(false);

  @HostListener('window:scroll')
  onWindowScroll() {
    this.isScrolled.set(window.scrollY > 24);
  }

  login() {
    this.accountService.login(this.creds).subscribe({
      next: () => {
        this.creds = {} as LoginCreds;
        this.router.navigateByUrl('/tasks');
        this.toast.success('Signed in');
      },
      error: (error) => {
        this.toast.error(error.error ?? 'Login failed');
      },
    });
  }

  logout() {
    this.accountService.logout();
    this.router.navigateByUrl('/');
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
}
