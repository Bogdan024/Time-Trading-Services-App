import { DatePipe } from '@angular/common';
import { Component, ElementRef, OnInit, ViewChild, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MessageService } from '../../core/services/message-service';
import { ToastService } from '../../core/services/toast-service';
import { Conversation, ConversationInboxType, Message, conversationStatusLabel } from '../../types/message';
import { Pagination } from '../../types/pagination';

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
  protected conversationPagination = signal<Pagination | null>(null);
  protected selectedConversation = signal<Conversation | null>(null);
  protected messages = signal<Message[]>([]);
  protected messagePagination = signal<Pagination | null>(null);
  protected selectedInbox = signal<ConversationInboxType>('TaskDirect');
  protected messageContent = '';
  protected loading = signal(false);
  protected loadingConversations = signal(false);
  protected loadingOlderMessages = signal(false);

  @ViewChild('messageThread') private messageThread?: ElementRef<HTMLElement>;

  private conversationPage = 1;
  private readonly conversationPageSize = 9;
  private readonly messagePageSize = 20;

  ngOnInit() {
    const conversationId = Number(this.route.snapshot.queryParamMap.get('conversationId')) || undefined;
    const taskId = Number(this.route.snapshot.queryParamMap.get('taskId')) || undefined;
    const groupId = Number(this.route.snapshot.queryParamMap.get('groupId')) || undefined;

    this.selectedInbox.set(groupId ? 'Group' : 'TaskDirect');
    this.loadConversations({ selectedConversationId: conversationId, selectedTaskId: taskId, selectedGroupId: groupId });
  }

  protected changeInbox(type: ConversationInboxType) {
    if (this.selectedInbox() === type) return;

    this.selectedInbox.set(type);
    this.conversationPage = 1;
    this.selectedConversation.set(null);
    this.messages.set([]);
    this.messagePagination.set(null);
    this.loadConversations();
  }

  protected selectConversation(conversation: Conversation) {
    this.selectedConversation.set(conversation);
    this.messages.set([]);
    this.messagePagination.set(null);
    this.loadMessages(conversation.id, 1);
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
        this.conversationPage = 1;
        this.scrollThreadToBottom();
        this.loadConversations({ selectedConversationId: conversation.id });
      },
      error: () => this.loading.set(false),
    });
  }

  protected deleteMessage(message: Message) {
    this.messageService.deleteMessage(message.id).subscribe({
      next: () => {
        this.messages.update((messages) => messages.filter((x) => x.id !== message.id));
        this.toast.success('Message deleted');
        this.loadConversations({ selectedConversationId: this.selectedConversation()?.id });
      },
    });
  }

  protected loadOlderMessages() {
    const conversation = this.selectedConversation();
    const pagination = this.messagePagination();

    if (!conversation || !pagination || pagination.currentPage >= pagination.totalPages) return;

    this.loadMessages(conversation.id, pagination.currentPage + 1, true);
  }

  protected nextConversationPage() {
    const pagination = this.conversationPagination();

    if (!pagination || pagination.currentPage >= pagination.totalPages) return;

    this.conversationPage = pagination.currentPage + 1;
    this.loadConversations({ selectedConversationId: this.selectedConversation()?.id });
  }

  protected previousConversationPage() {
    const pagination = this.conversationPagination();

    if (!pagination || pagination.currentPage <= 1) return;

    this.conversationPage = pagination.currentPage - 1;
    this.loadConversations({ selectedConversationId: this.selectedConversation()?.id });
  }

  protected canLoadOlderMessages() {
    const pagination = this.messagePagination();

    return !!pagination && pagination.currentPage < pagination.totalPages;
  }

  protected canGoPreviousConversationPage() {
    const pagination = this.conversationPagination();

    return !!pagination && pagination.currentPage > 1;
  }

  protected canGoNextConversationPage() {
    const pagination = this.conversationPagination();

    return !!pagination && pagination.currentPage < pagination.totalPages;
  }

  protected conversationTitle(conversation: Conversation) {
    return conversation.taskTitle ?? conversation.title ?? 'Conversation';
  }

  protected statusLabel(conversation: Conversation) {
    return conversationStatusLabel(conversation);
  }

  protected inboxDescription() {
    return this.selectedInbox() === 'Group'
      ? 'Group conversations stay open while you are part of the community space.'
      : 'Task conversations stay visible after an exchange closes, but sending is only available while work is in progress.';
  }

  private loadConversations(options: ConversationSelectionOptions = {}) {
    this.loadingConversations.set(true);
    this.messageService.getConversations(this.selectedInbox(), this.conversationPage, this.conversationPageSize).subscribe({
      next: (result) => {
        this.conversations.set(result.items);
        this.conversationPagination.set(result.metadata);
        this.loadingConversations.set(false);

        const selectedConversation = this.findSelectedConversation(result.items, options);

        if (selectedConversation) {
          this.selectedConversation.set(selectedConversation);

          if (!this.messages().length || this.messages()[0]?.conversationId !== selectedConversation.id) {
            this.loadMessages(selectedConversation.id, 1);
          }
        } else if (options.selectedTaskId) {
          this.loadTaskConversation(options.selectedTaskId);
        } else if (options.selectedConversationId || options.selectedGroupId) {
          this.selectedConversation.set(null);
          this.messages.set([]);
          this.messagePagination.set(null);
        }
      },
      error: () => this.loadingConversations.set(false),
    });
  }

  private loadTaskConversation(taskId: number) {
    this.messageService.getTaskConversation(taskId).subscribe({
      next: (conversation) => {
        this.selectedConversation.set(conversation);
        this.loadMessages(conversation.id, 1);
      },
    });
  }

  private loadMessages(conversationId: number, pageNumber: number, prepend = false) {
    const previousScrollHeight = prepend ? this.messageThread?.nativeElement.scrollHeight : 0;

    if (prepend) {
      this.loadingOlderMessages.set(true);
    }

    this.messageService.getConversationThread(conversationId, pageNumber, this.messagePageSize).subscribe({
      next: (result) => {
        this.messagePagination.set(result.metadata);
        this.messages.update((messages) => (prepend ? [...result.items, ...messages] : result.items));
        this.loadingOlderMessages.set(false);

        if (prepend) {
          this.preserveThreadScrollPosition(previousScrollHeight ?? 0);
        } else {
          this.scrollThreadToBottom();
        }
      },
      error: () => this.loadingOlderMessages.set(false),
    });
  }

  private findSelectedConversation(conversations: Conversation[], options: ConversationSelectionOptions) {
    if (options.selectedConversationId) {
      return conversations.find((x) => x.id === options.selectedConversationId);
    }

    if (options.selectedTaskId) {
      return conversations.find((x) => x.timeTaskId === options.selectedTaskId);
    }

    if (options.selectedGroupId) {
      return conversations.find((x) => x.groupId === options.selectedGroupId);
    }

    return null;
  }
  private scrollThreadToBottom() {
    setTimeout(() => {
      const thread = this.messageThread?.nativeElement;

      if (!thread) return;

      thread.scrollTop = thread.scrollHeight;
    });
  }

  private preserveThreadScrollPosition(previousScrollHeight: number) {
    setTimeout(() => {
      const thread = this.messageThread?.nativeElement;

      if (!thread) return;

      thread.scrollTop = thread.scrollHeight - previousScrollHeight;
    });
  }
}

type ConversationSelectionOptions = {
  selectedConversationId?: number;
  selectedTaskId?: number;
  selectedGroupId?: number;
};


