import { afterNextRender, ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiService } from '../../core/http/api.service';

@Component({
  selector: 'app-confirm-subscription',
  standalone: true,
  imports: [RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="confirm-page">
      @if (status() === 'loading') {
        <p class="confirm-page__msg">Confirming your subscription…</p>
      } @else if (status() === 'success') {
        <div class="confirm-page__card">
          <div class="confirm-page__icon">✓</div>
          <h1>You're subscribed!</h1>
          <p>Your subscription has been confirmed. You'll receive updates when new entries are published.</p>
        </div>
      } @else {
        <div class="confirm-page__card confirm-page__card--error">
          <div class="confirm-page__icon">✕</div>
          <h1>Confirmation failed</h1>
          <p>This confirmation link is invalid or has already been used.</p>
        </div>
      }
    </div>
  `,
  styles: [`
    .confirm-page {
      min-height: 100vh;
      display: flex;
      align-items: center;
      justify-content: center;
      background: var(--color-bg-primary, #fff);
      padding: 24px;
    }
    .confirm-page__msg { color: #888; font-family: sans-serif; }
    .confirm-page__card {
      max-width: 400px;
      text-align: center;
      font-family: sans-serif;
      padding: 40px 32px;
      border: 1px solid #e5e7eb;
      border-radius: 12px;
    }
    .confirm-page__card--error { border-color: #fecaca; }
    .confirm-page__icon {
      font-size: 48px;
      margin-bottom: 16px;
    }
    h1 { margin: 0 0 12px; font-size: 22px; font-weight: 600; }
    p { color: #555; margin: 0; line-height: 1.6; }
  `],
})
export class ConfirmSubscription {
  private route = inject(ActivatedRoute);
  private api = inject(ApiService);

  status = signal<'loading' | 'success' | 'error'>('loading');

  constructor() {
    afterNextRender(() => {
      const token = this.route.snapshot.queryParamMap.get('token');
      if (!token) { this.status.set('error'); return; }

      this.api.get<unknown>(`/subscribers/confirm/${token}`).subscribe({
        next: () => this.status.set('success'),
        error: () => this.status.set('error'),
      });
    });
  }
}
