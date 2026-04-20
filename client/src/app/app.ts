import { Component, inject, OnInit } from '@angular/core';
import { AccountService } from '../core/services/account-service';
import { Home } from '../features/home/home';
import { Nav } from '../layout/nav/nav';

@Component({
  selector: 'app-root',
  imports: [Nav, Home],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  private accountService = inject(AccountService);

  ngOnInit() {
    this.accountService.restoreUser();
  }
}
