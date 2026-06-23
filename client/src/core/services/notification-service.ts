import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { AppNotification } from '../../types/notification';

@Injectable({
  providedIn: 'root',
})
export class NotificationService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl;

  notifications = signal<AppNotification[]>([]);
  unreadCount = signal(0);

  loadNotifications() {
    this.http.get<AppNotification[]>(this.baseUrl + 'notifications').subscribe({
      next: (notifications) => {
        this.notifications.set(notifications);
        this.unreadCount.set(notifications.filter((notification) => !notification.isRead).length);
      },
    });
  }

  loadUnreadCount() {
    this.http.get<number>(this.baseUrl + 'notifications/unread-count').subscribe({
      next: (count) => this.unreadCount.set(count),
    });
  }

  addRealtimeNotification(notification: AppNotification) {
    this.notifications.update((notifications) => {
      if (notifications.some((x) => x.id === notification.id)) return notifications;

      return [notification, ...notifications].slice(0, 30);
    });
    this.unreadCount.update((count) => count + (notification.isRead ? 0 : 1));
  }

  markAsRead(notification: AppNotification) {
    if (notification.isRead) return;

    this.http.put<void>(this.baseUrl + 'notifications/' + notification.id + '/read', {}).subscribe({
      next: () => {
        const readAtUtc = new Date().toISOString();
        this.notifications.update((notifications) =>
          notifications.map((x) => (x.id === notification.id ? { ...x, isRead: true, readAtUtc } : x))
        );
        this.unreadCount.update((count) => Math.max(count - 1, 0));
      },
    });
  }

  markAllAsRead() {
    this.http.put<void>(this.baseUrl + 'notifications/read-all', {}).subscribe({
      next: () => {
        const readAtUtc = new Date().toISOString();
        this.notifications.update((notifications) => notifications.map((x) => ({ ...x, isRead: true, readAtUtc })));
        this.unreadCount.set(0);
      },
    });
  }

  reset() {
    this.notifications.set([]);
    this.unreadCount.set(0);
  }
}
