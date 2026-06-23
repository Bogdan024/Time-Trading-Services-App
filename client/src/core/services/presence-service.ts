import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { Message } from '../../types/message';
import { User } from '../../types/user';
import { ToastService } from './toast-service';

@Injectable({
  providedIn: 'root',
})
export class PresenceService {
  private toast = inject(ToastService);
  private hubUrl = environment.hubUrl;
  private hubConnection?: HubConnection;
  private currentToken?: string;

  onlineMemberIds = signal<string[]>([]);

  createHubConnection(user: User) {
    if (this.hubConnection?.state === HubConnectionState.Connected && this.currentToken === user.token) return;

    this.stopHubConnection();
    this.currentToken = user.token;

    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.hubUrl + 'presence', { accessTokenFactory: () => this.currentToken ?? user.token })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.on('UserOnline', (memberId: string) => {
      this.onlineMemberIds.update((memberIds) => (memberIds.includes(memberId) ? memberIds : [...memberIds, memberId]));
    });

    this.hubConnection.on('UserOffline', (memberId: string) => {
      this.onlineMemberIds.update((memberIds) => memberIds.filter((x) => x !== memberId));
    });

    this.hubConnection.on('GetOnlineUsers', (memberIds: string[]) => {
      this.onlineMemberIds.set(memberIds);
    });

    this.hubConnection.on('NewMessageReceived', (message: Message) => {
      this.toast.info(`New message from ${message.sender.displayName}`);
    });

    this.hubConnection.start().catch((error) => console.log(error));
  }

  stopHubConnection() {
    if (this.hubConnection && this.hubConnection.state !== HubConnectionState.Disconnected) {
      this.hubConnection.stop().catch((error) => console.log(error));
    }

    this.hubConnection = undefined;
    this.currentToken = undefined;
    this.onlineMemberIds.set([]);
  }
}
