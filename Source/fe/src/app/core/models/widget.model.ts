export interface WidgetEntry {
  id: string;
  title: string;
  contentHtml: string;
  tags: string[];
  version?: string;
  publishedAt: string;
}

export interface WidgetData {
  projectId: string;
  projectName: string;
  slug: string;
  accentColor: string;
  widgetPosition: string;
  entries: WidgetEntry[];
}
