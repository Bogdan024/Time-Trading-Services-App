export type NotificationType = 1 | 2 | 3 | 4;

export type AppNotification = {
  id: number;
  type: NotificationType;
  title: string;
  body: string;
  timeTaskId?: number;
  groupId?: number;
  conversationId?: number;
  createdAtUtc: string;
  readAtUtc?: string;
  isRead: boolean;
};
