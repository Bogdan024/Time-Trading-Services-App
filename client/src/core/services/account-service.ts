import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { tap } from 'rxjs';
import { LoginCreds, RegisterCreds, User } from '../../types/user';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class AccountService {
  private http = inject(HttpClient);
  currentUser = signal<User | null>(null);
  private baseUrl = environment.apiUrl;

  register(creds: RegisterCreds) {
    return this.http
      .post<User>(this.baseUrl + 'account/register', creds)
      .pipe(tap((user) => user && this.setCurrentUser(user)));
  }

  login(creds: LoginCreds) {
    return this.http
      .post<User>(this.baseUrl + 'account/login', creds)
      .pipe(tap((user) => user && this.setCurrentUser(user)));
  }

  setCurrentUser(user: User) {
    localStorage.setItem('user', JSON.stringify(user));
    this.currentUser.set(user);
  }

  restoreUser() {
    const userString = localStorage.getItem('user');

    if (!userString) {
      return;
    }

    this.currentUser.set(JSON.parse(userString));
  }

  logout() {
    localStorage.removeItem('user');
    this.currentUser.set(null);
  }
}
