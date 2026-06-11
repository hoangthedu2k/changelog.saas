import { computed, inject, Injectable, signal } from '@angular/core';
import { tap } from 'rxjs';
import { ApiService } from '../http/api.service';

export type SubscriptionPlan = 'Free' | 'Pro' | 'Team';
export type SubscriptionStatus = 'Active' | 'Canceled';

export interface SubscriptionDto {
  plan: SubscriptionPlan;
  status: SubscriptionStatus;
  currentPeriodEnd: string | null;
  stripeCustomerId: string | null;
  trialEndsAt: string;
  trialDaysLeft: number;
}

@Injectable({ providedIn: 'root' })
export class BillingService {
  private api = inject(ApiService);

  subscription = signal<SubscriptionDto | null>(null);
  loading = signal(false);

  isTrial = computed(() => {
    const s = this.subscription();
    return s !== null && s.plan === 'Free' && s.trialDaysLeft > 0;
  });

  trialDaysLeft = computed(() => this.subscription()?.trialDaysLeft ?? 0);

  loadSubscription() {
    this.loading.set(true);
    return this.api.get<SubscriptionDto>('/billing/subscription').pipe(
      tap(s => {
        this.subscription.set(s);
        this.loading.set(false);
      })
    );
  }

  createCheckout(priceId: string, successUrl: string, cancelUrl: string) {
    return this.api.post<{ url: string }>('/billing/checkout', { priceId, successUrl, cancelUrl });
  }

  createPortal(returnUrl: string) {
    return this.api.post<{ url: string }>('/billing/portal', { returnUrl });
  }
}
