import { TaskMember } from './task';

export type ModerationReport = {
  id: number;
  targetType: string;
  targetIntId?: number;
  targetStringId?: string;
  reason: string;
  details?: string;
  status: string;
  createdAtUtc: string;
  reviewedAtUtc?: string;
  moderatorNotes?: string;
  reporter?: TaskMember;
  reviewedBy?: TaskMember;
  targetTitle?: string;
  targetSummary?: string;
};

export type CreateReport = {
  targetType: ReportTargetType;
  targetIntId?: number;
  targetStringId?: string;
  reason: ReportReason;
  details?: string;
};

export enum ReportTargetType {
  Task,
  Group,
  Member,
  Message,
  Review,
}

export enum ReportReason {
  Spam,
  Abuse,
  Unsafe,
  Fraud,
  Inappropriate,
  Other,
}
