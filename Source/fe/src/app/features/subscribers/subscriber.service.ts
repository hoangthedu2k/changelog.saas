import { inject, Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { ApiService } from '../../core/http/api.service';

@Injectable({
  providedIn: 'root',
})
export class SubscriberService {
  private api = inject(ApiService);

  getCount(projectId: string): Observable<number> {
    return this.api
      .get<{ count: number }>(`/subscribers/count?projectId=${projectId}`)
      .pipe(map(r => r.count));
  }
}
