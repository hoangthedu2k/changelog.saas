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
  isTrialing: boolean;
  trialPlan: SubscriptionPlan | null;
  trialDaysLeft: number;
}

@Injectable({ providedIn: 'root' })
export class BillingService {
  private api = inject(ApiService);

  subscription = signal<SubscriptionDto | null>(null);
  loading = signal(false);

  isTrial = computed(() => this.subscription()?.isTrialing ?? false);
  trialDaysLeft = computed(() => this.subscription()?.trialDaysLeft ?? 0);
  trialPlan = computed(() => this.subscription()?.trialPlan ?? null);

  loadSubscription() {
    this.loading.set(true);
    return this.api.get<SubscriptionDto>('/billing/subscription').pipe(
      tap(s => {
        this.subscription.set(s);
        this.loading.set(false);
      })
    );
  }

  startTrial(plan: 'Pro' | 'Team') {
    return this.api.post<void>('/billing/trial', { plan }).pipe(
      tap(() => this.loadSubscription().subscribe())
    );
  }

  createCheckout(priceId: string, successUrl: string, cancelUrl: string) {
    return this.api.post<{ url: string }>('/billing/checkout', { priceId, successUrl, cancelUrl });
  }

  createPortal(returnUrl: string) {
    return this.api.post<{ url: string }>('/billing/portal', { returnUrl });
  }
}
