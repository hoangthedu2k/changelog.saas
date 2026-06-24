import { ChangeDetectionStrategy, Component, computed, effect, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../../core/http/api.service';
import { ProjectService } from '../../../core/services/project.service';
import { Subscriber, SubscriberStatus } from '../../../core/models/subscriber.model';

interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasPrev: boolean;
  hasNext: boolean;
}

const PAGE_SIZE = 20;

@Component({
  selector: 'app-subscriber-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './subscriber-list.html',
  styleUrl: './subscriber-list.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SubscriberList {
  private api = inject(ApiService);
  projectService = inject(ProjectService);

  result = signal<PagedResult<Subscriber> | null>(null);
  loading = signal(false);
  page = signal(1);

  statusFilter = signal<SubscriberStatus | ''>('');

  subscribers = computed(() => this.result()?.items ?? []);
  totalCount = computed(() => this.result()?.totalCount ?? 0);
  totalPages = computed(() => this.result()?.totalPages ?? 0);
  hasPrev = computed(() => this.result()?.hasPrev ?? false);
  hasNext = computed(() => this.result()?.hasNext ?? false);

  readonly maxSubscribersLabel = '2000';

  readonly statusOptions: { label: string; value: SubscriberStatus | '' }[] = [
    { label: 'All', value: '' },
    { label: 'Verified', value: 'Verified' },
    { label: 'Pending', value: 'Pending' },
    { label: 'Unsubscribed', value: 'Unsubscribed' },
  ];

  constructor() {
    effect(() => {
      const project = this.projectService.activeProject();
      const page = this.page();
      const status = this.statusFilter();
      if (!project) return;

      this.load(project.id, page, status);
    });
  }

  private load(projectId: string, page: number, status: string) {
    this.loading.set(true);
    let url = `/subscribers?projectId=${projectId}&page=${page}&pageSize=${PAGE_SIZE}`;
    if (status) url += `&status=${status}`;

    this.api.get<PagedResult<Subscriber>>(url).subscribe({
      next: res => { this.result.set(res); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  setStatus(value: SubscriberStatus | '') {
    this.statusFilter.set(value);
    this.page.set(1);
  }

  goTo(p: number) {
    this.page.set(p);
  }
}
