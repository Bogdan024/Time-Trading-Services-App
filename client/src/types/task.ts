import { ServiceCategory } from './member';

export type TaskLocationMode = 1 | 2 | 3;
export type TimeTaskStatus = 1 | 2 | 3 | 4;

export type TaskMember = {
  id: string;
  displayName: string;
  avatarUrl?: string;
  city?: string;
  countryCode?: string;
};

export type TimeTask = {
  id: number;
  title: string;
  description: string;
  estimatedHours: number;
  locationMode: TaskLocationMode;
  city?: string;
  countryCode?: string;
  createdAtUtc: string;
  updatedAtUtc?: string;
  dueAtUtc?: string;
  completedAtUtc?: string;
  status: TimeTaskStatus;
  serviceCategory: ServiceCategory;
  postedByMember: TaskMember;
  acceptedByMember?: TaskMember;
};

export type CreateTimeTask = {
  title: string;
  description: string;
  serviceCategoryId: number;
  estimatedHours: number;
  locationMode: TaskLocationMode;
  city?: string;
  countryCode?: string;
  dueAtUtc?: string;
};

export type UpdateTimeTask = CreateTimeTask;

export type TimeTransaction = {
  id: number;
  timeTaskId: number;
  taskTitle: string;
  hours: number;
  createdAtUtc: string;
  note?: string;
  fromMember: TaskMember;
  toMember: TaskMember;
};

export class TaskParams {
  serviceCategoryId?: number;
  locationMode?: TaskLocationMode;
  city?: string;
  countryCode?: string;
  minCredits?: number;
  maxCredits?: number;
  dueSoon = false;
  orderBy = 'newest';
  pageNumber = 1;
  pageSize = 9;
}
