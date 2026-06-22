import { TaskMember } from './task';

export type CreateReview = {
  rating: number;
  comment?: string;
};

export type MemberReview = {
  id: number;
  timeTaskId: number;
  taskTitle: string;
  rating: number;
  comment?: string;
  createdAtUtc: string;
  reviewer: TaskMember;
  reviewedMember: TaskMember;
};

export type ReviewSummary = {
  averageRating: number;
  reviewCount: number;
};
