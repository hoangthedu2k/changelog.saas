import { inject, Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { ApiService } from '../../core/http/api.service';
import { Entry } from '../../core/models/entry.model';

export interface DashboardStats {
  total: number;
  published: number;
  draft: number;
}

@Injectable({
  providedIn: 'root',
})
export class DashboardService {
  private api = inject(ApiService);

  getStats(projectId: string): Observable<DashboardStats> {
    return this.api.get<Entry[]>(`/entries?projectId=${projectId}`).pipe(
      map(entries => ({
        total: entries.length,
        published: entries.filter(e => e.status === 'Published').length,
        draft: entries.filter(e => e.status === 'Draft').length,
      }))
    );
  }

  getRecentEntries(projectId: string, limit = 5): Observable<Entry[]> {
    return this.api.get<Entry[]>(`/entries?projectId=${projectId}`).pipe(
      map(entries => entries.slice(0, limit))
    );
  }
}
