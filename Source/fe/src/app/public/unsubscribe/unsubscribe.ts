import { afterNextRender, ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ApiService } from '../../core/http/api.service';

@Component({
  selector: 'app-unsubscribe',
  standalone: true,
  imports: [],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="unsub-page">
      @if (status() === 'loading') {
        <p class="unsub-page__msg">Processing…</p>
      } @else if (status() === 'success') {
        <div class="unsub-page__card">
          <div class="unsub-page__icon">✓</div>
          <h1>Unsubscribed</h1>
          <p>You have been removed from this changelog's mailing list.</p>
        </div>
      } @else {
        <div class="unsub-page__card unsub-page__card--error">
          <div class="unsub-page__icon">✕</div>
          <h1>Link invalid</h1>
          <p>This unsubscribe link is invalid or has already been used.</p>
        </div>
      }
    </div>
  `,
  styles: [`
    .unsub-page {
      min-height: 100vh;
      display: flex;
      align-items: center;
      justify-content: center;
      background: var(--color-bg-primary, #fff);
      padding: 24px;
    }
    .unsub-page__msg { color: #888; font-family: sans-serif; }
    .unsub-page__card {
      max-width: 400px;
      text-align: center;
      font-family: sans-serif;
      padding: 40px 32px;
      border: 1px solid #e5e7eb;
      border-radius: 12px;
    }
    .unsub-page__card--error { border-color: #fecaca; }
    .unsub-page__icon { font-size: 48px; margin-bottom: 16px; }
    h1 { margin: 0 0 12px; font-size: 22px; font-weight: 600; }
    p { color: #555; margin: 0; line-height: 1.6; }
  `],
})
export class Unsubscribe {
  private route = inject(ActivatedRoute);
  private api = inject(ApiService);

  status = signal<'loading' | 'success' | 'error'>('loading');

  constructor() {
    afterNextRender(() => {
      const token = this.route.snapshot.queryParamMap.get('token');
      if (!token) { this.status.set('error'); return; }

      this.api.get<unknown>(`/subscribers/unsubscribe/${token}`).subscribe({
        next: () => this.status.set('success'),
        error: () => this.status.set('error'),
      });
    });
  }
}
