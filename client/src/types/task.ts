import { ServiceCategory } from './member';

export type TaskLocationMode = 1 | 2 | 3;
export type TimeTaskStatus = 1 | 2 | 3 | 4;
export type TaskApplicationStatus = 1 | 2 | 3 | 4;

export type TaskLocation = {
  city: string;
  countryCode: string;
  formattedAddress: string;
  latitude: number;
  longitude: number;
  placeId?: string;
};

export type TaskMember = {
  id: string;
  displayName: string;
  avatarUrl?: string;
  city?: string;
  countryCode?: string;
};

export type TaskApplication = {
  id: number;
  timeTaskId: number;
  status: TaskApplicationStatus;
  message?: string;
  createdAtUtc: string;
  updatedAtUtc?: string;
  matchesTaskCategory: boolean;
  applicantAverageRating?: number;
  applicantReviewCount: number;
  applicant: TaskMember;
};

export type TimeTask = {
  id: number;
  title: string;
  description: string;
  estimatedHours: number;
  locationMode: TaskLocationMode;
  city?: string;
  countryCode?: string;
  formattedAddress?: string;
  placeId?: string;
  latitude?: number | null;
  longitude?: number | null;
  createdAtUtc: string;
  updatedAtUtc?: string;
  dueAtUtc?: string;
  completedAtUtc?: string;
  status: TimeTaskStatus;
  serviceCategory: ServiceCategory;
  postedByMember: TaskMember;
  acceptedByMember?: TaskMember;
  applicationCount: number;
  hasCurrentUserApplied: boolean;
  currentUserApplicationStatus?: TaskApplicationStatus;
};

export type CreateTimeTask = {
  title: string;
  description: string;
  serviceCategoryId: number;
  estimatedHours: number;
  locationMode: TaskLocationMode;
  city: string;
  countryCode: string;
  formattedAddress: string;
  latitude: number;
  longitude: number;
  placeId?: string;
  dueAtUtc?: string;
};

export type UpdateTimeTask = CreateTimeTask;

export type CreateTaskApplication = {
  message?: string;
};

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
  minPosterRating?: number;
  orderBy = 'newest';
  pageNumber = 1;
  pageSize = 9;
}

