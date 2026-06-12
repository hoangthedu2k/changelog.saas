import { BaseEntity } from './baseentity.model';

export type SubscriberStatus = 'Pending' | 'Verified' | 'Unsubscribed';

export interface Subscriber extends BaseEntity {
  projectId: string;
  email: string;
  status: SubscriberStatus;
  confirmedAt: string | null;
}

export interface SubscribeRequest {
  projectId: string;
  email: string;
}
