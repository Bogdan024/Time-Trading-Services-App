export type ServiceCategory = {
  id: number;
  key: string;
  name: string;
};

export type MemberSkill = {
  id: number;
  memberId: string;
  serviceCategoryId: number;
  note?: string;
  serviceCategory: ServiceCategory;
};

export type MemberNeed = {
  id: number;
  memberId: string;
  serviceCategoryId: number;
  note?: string;
  serviceCategory: ServiceCategory;
};

export type MemberAvailabilitySlot = {
  id: number;
  memberId: string;
  dayOfWeek: number;
  startHour: number;
  endHour: number;
  mode: number;
};

export type Photo = {
  id: number;
  url: string;
  publicId?: string;
  memberId: string;
};

export type Member = {
  id: string;
  displayName: string;
  about?: string;
  avatarUrl?: string;
  city?: string;
  countryCode?: string;
  isProfilePublic: boolean;
  createdAtUtc: string;
  lastActiveAtUtc: string;
  photos: Photo[];
  offeredSkills: MemberSkill[];
  needsHelpWith: MemberNeed[];
  availabilitySlots: MemberAvailabilitySlot[];
};
