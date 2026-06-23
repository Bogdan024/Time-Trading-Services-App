import { DatePipe } from '@angular/common';
import { Component, HostListener, effect, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink, RouterLinkActive, UrlTree } from '@angular/router';
import { AccountService } from '../../core/services/account-service';
import { BalanceService } from '../../core/services/balance-service';
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
  protected balanceService = inject(BalanceService);
  protected notificationService = inject(NotificationService);
  private router = inject(Router);
  private toast = inject(ToastService);
  protected creds = {} as LoginCreds;
  protected readonly isScrolled = signal(false);
  protected readonly loginLoading = signal(false);

  constructor() {
    effect(() => {
      if (this.accountService.currentUser()) {
        this.balanceService.refreshBalance();
      } else {
        this.balanceService.reset();
      }
    });
  }

  @HostListener('window:scroll')
  onWindowScroll() {
    this.isScrolled.set(window.scrollY > 24);
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
    this.balanceService.refreshBalance();

    if (notification.type === 5) {
      const userId = this.accountService.currentUser()?.id;

      if (userId) {
        this.navigateOrReload(this.router.createUrlTree(['/members', userId]));
      }

      return;
    }

    if (notification.conversationId) {
      this.navigateOrReload(this.router.createUrlTree(['/messages'], { queryParams: { conversationId: notification.conversationId } }));
      return;
    }

    if (notification.timeTaskId) {
      this.navigateOrReload(this.router.createUrlTree(['/tasks', notification.timeTaskId]));
      return;
    }

    if (notification.groupId) {
      this.navigateOrReload(this.router.createUrlTree(['/groups', notification.groupId]));
    }
  }

  private navigateOrReload(targetTree: UrlTree) {
    const targetUrl = this.router.serializeUrl(targetTree);

    if (this.router.url === targetUrl) {
      this.router.navigateByUrl('/', { skipLocationChange: true }).then(() => this.router.navigateByUrl(targetUrl));
      return;
    }

    this.router.navigateByUrl(targetUrl);
  }
}

