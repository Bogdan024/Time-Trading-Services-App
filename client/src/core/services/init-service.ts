import { inject, Injectable } from '@angular/core';
import { of } from 'rxjs';
import { AccountService } from './account-service';

@Injectable({
  providedIn: 'root',
})
export class InitService {
  private accountService = inject(AccountService);

  init() {
    this.accountService.restoreUser();
    return of(null);
  }
}
