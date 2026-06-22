import { TaskMember } from './task';

export type ConversationType = 1 | 2;
export type ConversationInboxType = 'TaskDirect' | 'Group';

export type Conversation = {
  id: number;
  type: ConversationType;
  timeTaskId?: number;
  groupId?: number;
  taskTitle?: string;
  title?: string;
  createdAtUtc: string;
  closedAtUtc?: string;
  canSendMessages: boolean;
  unreadCount: number;
  latestMessage?: string;
  latestMessageAtUtc?: string;
  otherMember?: TaskMember;
};

export type Message = {
  id: string;
  conversationId: number;
  content: string;
  createdAtUtc: string;
  sender: TaskMember;
  currentUserSender: boolean;
};

export type CreateMessage = {
  content: string;
};

export function conversationStatusLabel(conversation: Conversation) {
  if (conversation.canSendMessages) return 'Open';
  if (conversation.closedAtUtc) return 'Read-only';
  return 'Closed';
}



