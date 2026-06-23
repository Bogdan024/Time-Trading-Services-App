import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { from, throwError } from 'rxjs';
import { environment } from '../../environments/environment';
import { Conversation, ConversationInboxType, Message } from '../../types/message';
import { PaginatedResult } from '../../types/pagination';
import { AccountService } from './account-service';

@Injectable({
  providedIn: 'root',
})
export class MessageService {
  private http = inject(HttpClient);
  private accountService = inject(AccountService);
  private baseUrl = environment.apiUrl;
  private hubUrl = environment.hubUrl;
  private hubConnection?: HubConnection;
  private hubConnectionStarted?: Promise<void>;

  messageThread = signal<Message[]>([]);

  createHubConnection(conversationId: number) {
    const user = this.accountService.currentUser();

    if (!user) return;

    this.stopHubConnection();
    this.messageThread.set([]);

    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.hubUrl + 'messages?conversationId=' + conversationId, { accessTokenFactory: () => user.token })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.on('NewMessage', (message: Message) => {
      this.messageThread.update((messages) => [
        ...messages,
        { ...message, currentUserSender: message.sender.id === user.id },
      ]);
    });

    this.hubConnectionStarted = this.hubConnection.start().catch((error) => {
      console.log(error);
      throw error;
    });
  }

  stopHubConnection() {
    if (this.hubConnection && this.hubConnection.state !== HubConnectionState.Disconnected) {
      this.hubConnection.stop().catch((error) => console.log(error));
    }

    this.hubConnection = undefined;
    this.hubConnectionStarted = undefined;
  }

  sendRealtimeMessage(content: string) {
    if (!this.hubConnection || !this.hubConnectionStarted) {
      return throwError(() => new Error('No active message connection'));
    }

    return from(this.hubConnectionStarted.then(() => this.hubConnection?.invoke('SendMessage', { content })));
  }

  getConversations(type: ConversationInboxType, pageNumber = 1, pageSize = 9) {
    const params = new HttpParams()
      .set('type', type)
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);

    return this.http.get<PaginatedResult<Conversation>>(this.baseUrl + 'messages/conversations', { params });
  }

  getConversationThread(conversationId: number, pageNumber = 1, pageSize = 20) {
    const params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);

    return this.http.get<PaginatedResult<Message>>(this.baseUrl + 'messages/conversations/' + conversationId, { params });
  }

  getTaskConversation(taskId: number) {
    return this.http.get<Conversation>(this.baseUrl + 'messages/tasks/' + taskId);
  }

  getTaskConversationThread(taskId: number, pageNumber = 1, pageSize = 20) {
    const params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);

    return this.http.get<PaginatedResult<Message>>(this.baseUrl + 'messages/tasks/' + taskId + '/thread', { params });
  }

  sendConversationMessage(conversationId: number, content: string) {
    return this.http.post<Message>(this.baseUrl + 'messages/conversations/' + conversationId, { content });
  }

  sendTaskMessage(taskId: number, content: string) {
    return this.http.post<Message>(this.baseUrl + 'messages/tasks/' + taskId, { content });
  }

  deleteMessage(messageId: string) {
    return this.http.delete<void>(this.baseUrl + 'messages/' + messageId);
  }
}
