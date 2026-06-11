import { BaseEntity } from './baseentity.model';

export type WidgetPosition = 'bl' | 'br' | 'tl' | 'tr';

export interface Project extends BaseEntity {
  userId: string;
  name: string;
  slug: string;
  customDomain?: string;
  accentColor: string;
  isPublic: boolean;
  widgetPosition: WidgetPosition;
}

export interface CreateProjectRequest {
  name: string;
  slug: string;
  customDomain?: string;
  accentColor: string;
  isPublic: boolean;
  widgetPosition: WidgetPosition;
}

export interface UpdateProjectRequest {
  id: string;
  name: string;
  slug: string;
  customDomain?: string;
  accentColor: string;
  isPublic: boolean;
  widgetPosition: WidgetPosition;
}

export interface GetProjectsRequest {
  name?: string;
  slug?: string;
}
