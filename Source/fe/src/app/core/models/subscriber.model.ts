import { BaseEntity } from './baseentity.model';

export interface Subscriber extends BaseEntity {
  projectId: string;
  email: string;
  confirmedAt?: string;

}

export interface SubscribeRequest {
  projectId: string;
  email: string;
}
