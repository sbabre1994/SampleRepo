export interface PlannedUpgrade {
  id: number;
  version: string;
  plannedDateTime: string;
  description?: string;
  createdAt: string;
  isActive: boolean;
}

export interface UserUpgradePreference {
  id: number;
  userId: string;
  userEmail: string;
  plannedUpgradeId: number;
  preferredDateTime?: string;
  createdAt: string;
  updatedAt?: string;
  emailNotificationSent: boolean;
  jobScheduled: boolean;
}

export interface UpgradeNotificationDto {
  plannedUpgradeId: number;
  version: string;
  plannedDateTime: string;
  description?: string;
  userPreferredDateTime?: string;
  hasUserPreference: boolean;
}

export interface SetPreferenceRequest {
  plannedUpgradeId: number;
  userId: string;
  userEmail: string;
  preferredDateTime?: string;
}

export interface UpgradeJobPayload {
  plannedUpgradeId: number;
  userId: string;
  userEmail: string;
  version: string;
  scheduledDateTime: string;
}