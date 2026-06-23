import { TaskMember } from './task';

export type CommunityGroup = {
  id: number;
  name: string;
  description: string;
  theme?: string;
  city?: string;
  countryCode?: string;
  moderationStatus: string;
  rejectionReason?: string;
  createdAtUtc: string;
  memberCount: number;
  isMember: boolean;
  isOwner: boolean;
  conversationId?: number;
  owner?: TaskMember;
};

export type CreateGroup = {
  name: string;
  description: string;
  theme?: string;
  city?: string;
  countryCode?: string;
};

export type PendingGroup = {
  id: number;
  name: string;
  description: string;
  theme?: string;
  city?: string;
  countryCode?: string;
  moderationStatus: string;
  createdAtUtc: string;
  owner?: TaskMember;
};
