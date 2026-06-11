import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '../../core/http/api.service';
import {
  CreateEntryRequest,
  Entry,
  GetEntriesRequest,
  UpdateEntryRequest,
} from '../../core/models/entry.model';

@Injectable({
  providedIn: 'root',
})
export class EntriesService {
  private api = inject(ApiService);

  getById(id: string): Observable<Entry> {
    return this.api.get<Entry>(`/entries/${id}`);
  }

  getAll(params: GetEntriesRequest): Observable<Entry[]> {
    const query = new URLSearchParams({ projectId: params.projectId });
    if (params.status) query.set('status', params.status);
    if (params.title) query.set('title', params.title);
    return this.api.get<Entry[]>(`/entries?${query}`);
  }

  // BE returns Guid (string) on create
  create(request: CreateEntryRequest): Observable<string> {
    return this.api.post<string>('/entries', request);
  }

  update(id: string, request: UpdateEntryRequest): Observable<Entry> {
    return this.api.put<Entry>(`/entries/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.api.delete<void>(`/entries/${id}`);
  }

  // BE returns Guid on publish
  publish(id: string): Observable<string> {
    return this.api.post<string>(`/entries/${id}/publish`, {});
  }
}
