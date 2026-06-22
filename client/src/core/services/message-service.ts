import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { Conversation, Message } from '../../types/message';

@Injectable({
  providedIn: 'root',
})
export class MessageService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl;

  getConversations() {
    return this.http.get<Conversation[]>(this.baseUrl + 'messages/conversations');
  }

  getConversationThread(conversationId: number) {
    return this.http.get<Message[]>(this.baseUrl + 'messages/conversations/' + conversationId);
  }

  getTaskConversation(taskId: number) {
    return this.http.get<Conversation>(this.baseUrl + 'messages/tasks/' + taskId);
  }

  getTaskConversationThread(taskId: number) {
    return this.http.get<Message[]>(this.baseUrl + 'messages/tasks/' + taskId + '/thread');
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
