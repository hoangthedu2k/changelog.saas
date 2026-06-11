import { BaseEntity } from './baseentity.model';

export type EntryStatus = 'Draft' | 'Published';

export interface Entry extends BaseEntity {
  projectId: string;
  title: string;
  contentHtml: string;
  tags: string[];
  status: EntryStatus;
  version?: string;
  publishedAt?: string;
}

export interface CreateEntryRequest {
  projectId: string;
  title: string;
  contentHtml: string;
  tags: string[];
  version?: string;
}

export interface UpdateEntryRequest {
  id: string;
  title: string;
  contentHtml: string;
  tags: string[];
  version?: string;
}

export interface GetEntriesRequest {
  projectId: string;
  status?: EntryStatus;
  title?: string;
}
