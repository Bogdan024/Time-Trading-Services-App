import { Component, HostListener, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AccountService } from '../../core/services/account-service';
import { LoginCreds } from '../../types/user';

@Component({
  selector: 'app-nav',
  imports: [FormsModule],
  templateUrl: './nav.html',
  styleUrl: './nav.css',
})
export class Nav {
  protected accountService = inject(AccountService);
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
      },
      error: (error) => alert(error.message),
    });
  }

  logout() {
    this.accountService.logout();
  }
}
