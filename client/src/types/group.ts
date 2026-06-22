import { TaskMember } from './task';

export type CommunityGroup = {
  id: number;
  name: string;
  description: string;
  theme?: string;
  city?: string;
  countryCode?: string;
  isPublic: boolean;
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
  isPublic: boolean;
};
