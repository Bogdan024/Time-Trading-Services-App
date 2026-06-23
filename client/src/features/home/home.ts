import { Component, effect, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AccountService } from '../../core/services/account-service';
import { Register } from '../account/register/register';

@Component({
  selector: 'app-home',
  imports: [Register, RouterLink],
  templateUrl: './home.html',
  styleUrl: './home.css',
})
export class Home {
  protected accountService = inject(AccountService);
  protected registerMode = signal(false);

  constructor() {
    effect(() => {
      if (this.accountService.currentUser()) {
        this.registerMode.set(false);
      }
    });
  }

  showRegister(value: boolean) {
    this.registerMode.set(value && !this.accountService.currentUser());
  }
}

