import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { Conversation, ConversationInboxType, Message } from '../../types/message';
import { PaginatedResult } from '../../types/pagination';

@Injectable({
  providedIn: 'root',
})
export class MessageService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl;

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
