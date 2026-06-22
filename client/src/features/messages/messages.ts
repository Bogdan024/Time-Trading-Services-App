import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MessageService } from '../../core/services/message-service';
import { ToastService } from '../../core/services/toast-service';
import { Conversation, Message, conversationStatusLabel } from '../../types/message';

@Component({
  selector: 'app-messages',
  imports: [DatePipe, FormsModule, RouterLink],
  templateUrl: './messages.html',
  styleUrl: './messages.css',
})
export class Messages implements OnInit {
  private messageService = inject(MessageService);
  private route = inject(ActivatedRoute);
  private toast = inject(ToastService);

  protected conversations = signal<Conversation[]>([]);
  protected selectedConversation = signal<Conversation | null>(null);
  protected messages = signal<Message[]>([]);
  protected messageContent = '';
  protected loading = signal(false);

  ngOnInit() {
    const conversationId = Number(this.route.snapshot.queryParamMap.get('conversationId')) || undefined;
    const taskId = Number(this.route.snapshot.queryParamMap.get('taskId')) || undefined;

    this.loadConversations(conversationId, taskId);
  }

  protected selectConversation(conversation: Conversation) {
    this.selectedConversation.set(conversation);
    this.loadMessages(conversation.id);
  }

  protected sendMessage() {
    const conversation = this.selectedConversation();
    const content = this.messageContent.trim();

    if (!conversation?.canSendMessages || !content) return;

    this.loading.set(true);
    this.messageService.sendConversationMessage(conversation.id, content).subscribe({
      next: (message) => {
        this.messages.update((messages) => [...messages, message]);
        this.messageContent = '';
        this.loading.set(false);
        this.loadConversations(conversation.id);
      },
      error: () => this.loading.set(false),
    });
  }

  protected deleteMessage(message: Message) {
    this.messageService.deleteMessage(message.id).subscribe({
      next: () => {
        this.messages.update((messages) => messages.filter((x) => x.id !== message.id));
        this.toast.success('Message deleted');
        this.loadConversations(this.selectedConversation()?.id);
      },
    });
  }

  protected conversationTitle(conversation: Conversation) {
    return conversation.taskTitle ?? conversation.title ?? 'Conversation';
  }

  protected statusLabel(conversation: Conversation) {
    return conversationStatusLabel(conversation);
  }

  private loadConversations(selectedConversationId?: number, selectedTaskId?: number) {
    this.messageService.getConversations().subscribe({
      next: (conversations) => {
        this.conversations.set(conversations);

        const selectedConversation = selectedConversationId
          ? conversations.find((x) => x.id === selectedConversationId)
          : selectedTaskId
            ? conversations.find((x) => x.timeTaskId === selectedTaskId)
            : null;

        if (selectedConversation) {
          this.selectConversation(selectedConversation);
        } else if (selectedConversationId || selectedTaskId) {
          this.selectedConversation.set(null);
          this.messages.set([]);
        }
      },
    });
  }

  private loadMessages(conversationId: number) {
    this.messageService.getConversationThread(conversationId).subscribe({
      next: (messages) => this.messages.set(messages),
    });
  }
}
