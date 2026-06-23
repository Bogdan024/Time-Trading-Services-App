import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { LoginCreds, RegisterCreds, User } from '../../types/user';
import { NotificationService } from './notification-service';
import { PresenceService } from './presence-service';

@Injectable({
  providedIn: 'root',
})
export class AccountService {
  private http = inject(HttpClient);
  private notificationService = inject(NotificationService);
  private presenceService = inject(PresenceService);
  currentUser = signal<User | null>(null);
  private baseUrl = environment.apiUrl;

  register(creds: RegisterCreds) {
    return this.http
      .post<User>(this.baseUrl + 'account/register', creds, { withCredentials: true })
      .pipe(tap((user) => user && this.setCurrentUser(user)));
  }

  login(creds: LoginCreds) {
    return this.http
      .post<User>(this.baseUrl + 'account/login', creds, { withCredentials: true })
      .pipe(tap((user) => user && this.setCurrentUser(user)));
  }

  refreshToken() {
    return this.http
      .post<User>(this.baseUrl + 'account/refresh-token', {}, { withCredentials: true })
      .pipe(tap((user) => user && this.setCurrentUser(user)));
  }

  setCurrentUser(user: User) {
    user.roles = user.roles?.length ? user.roles : this.getDecodedRoles(user.token);
    this.currentUser.set(user);
    this.notificationService.loadNotifications();
    this.presenceService.createHubConnection(user);
  }

  hasRole(roles: string[]) {
    return this.currentUser()?.roles?.some((role) => roles.includes(role)) ?? false;
  }

  logout() {
    this.http.post(this.baseUrl + 'account/revoke-token', {}, { withCredentials: true }).subscribe();
    this.clearCurrentUser();
  }

  clearCurrentUser() {
    localStorage.removeItem('taskFilters');
    this.currentUser.set(null);
    this.presenceService.stopHubConnection();
    this.notificationService.reset();
  }

  private getDecodedRoles(token: string) {
    try {
      const tokenPayload = JSON.parse(atob(token.split('.')[1]));
      const roles = tokenPayload.role ?? tokenPayload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ?? [];

      return Array.isArray(roles) ? roles : [roles];
    } catch {
      return [];
    }
  }
}
