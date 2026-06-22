import { Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AccountService } from '../../core/services/account-service';
import { LoginCreds } from '../../types/user';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { ToastService } from '../../core/services/toast-service';

@Component({
  selector: 'app-nav',
  imports: [FormsModule, RouterLink, RouterLinkActive],
  templateUrl: './nav.html',
  styleUrl: './nav.css',
})
export class Nav implements OnInit, OnDestroy {
  protected accountService = inject(AccountService);
  private router = inject(Router);
  private toast = inject(ToastService);
  protected creds = {} as LoginCreds;
  protected readonly isScrolled = signal(false);

  // Plain passive listener (not @HostListener) so high-frequency scroll events
  // don't churn change detection. The signal only flips at the threshold, so a
  // change-detection tick is scheduled at most twice per scroll direction.
  private readonly onWindowScroll = () => this.isScrolled.set(window.scrollY > 24);

  ngOnInit() {
    window.addEventListener('scroll', this.onWindowScroll, { passive: true });
  }

  ngOnDestroy() {
    window.removeEventListener('scroll', this.onWindowScroll);
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
}
