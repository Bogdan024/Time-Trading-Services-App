import { inject, Injectable } from '@angular/core';
import { catchError, of } from 'rxjs';
import { AccountService } from './account-service';

@Injectable({
  providedIn: 'root',
})
export class InitService {
  private accountService = inject(AccountService);

  init() {
    return this.accountService.refreshToken().pipe(
      catchError(() => {
        this.accountService.clearCurrentUser();
        return of(null);
      }),
    );
  }
}
