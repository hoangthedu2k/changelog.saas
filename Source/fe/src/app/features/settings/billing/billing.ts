import { afterNextRender, ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DestroyRef } from '@angular/core';
import { BillingService } from '../../../core/services/billing.service';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-billing',
  standalone: true,
  imports: [CommonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './billing.html',
  styleUrl: './billing.scss',
})
export class Billing {
  billing = inject(BillingService);
  private route = inject(ActivatedRoute);
  private destroyRef = inject(DestroyRef);

  upgradePrompt = signal(false);
  startingTrial: string | null = null;
  upgrading: string | null = null;

  readonly plans = [
    {
      id: 'free',
      name: 'Free',
      price: '$0',
      tagline: 'For indie hackers just starting out',
      period: '/month',
      popular: false,
      features: [
        { label: '1 project', available: true },
        { label: 'Up to 5 entries', available: true },
        { label: '100 subscribers', available: true },
        { label: 'Embed widget', available: true },
        { label: 'Custom domain', available: false },
        { label: 'Remove branding', available: false },
      ],
      priceId: null,
    },
    {
      id: 'pro',
      name: 'Pro',
      price: '$9',
      tagline: 'For growing SaaS products',
      period: '/month',
      popular: true,
      features: [
        { label: '3 projects', available: true },
        { label: 'Unlimited entries', available: true },
        { label: '2,000 subscribers', available: true },
        { label: 'Custom domain', available: true },
        { label: 'Remove branding', available: true },
        { label: 'API access', available: false },
      ],
      priceId: environment.stripe.proPriceId,
    },
    {
      id: 'team',
      name: 'Team',
      price: '$29',
      tagline: 'For teams & multiple products',
      period: '/month',
      popular: false,
      features: [
        { label: 'Unlimited projects', available: true },
        { label: 'Unlimited entries', available: true },
        { label: 'Unlimited subscribers', available: true },
        { label: 'Custom domain', available: true },
        { label: 'Remove branding', available: true },
        { label: 'API access', available: true },
      ],
      priceId: environment.stripe.teamPriceId,
    },
  ];

  constructor() {
    this.route.queryParamMap
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(p => this.upgradePrompt.set(p.get('upgrade') === 'true'));

    afterNextRender(() => {
      this.billing.loadSubscription().subscribe();
    });
  }

  get currentPlan() {
    return this.billing.subscription()?.plan?.toLowerCase() ?? 'free';
  }

  canStartTrial(planId: string) {
    const sub = this.billing.subscription();
    if (!sub) return false;
    // Already trialing or paid — no trial button
    if (sub.isTrialing || sub.plan !== 'Free') return false;
    // Already trialed this plan before? (trialPlan was set but expired)
    // We allow re-trial only if never trialed — handled by BE
    return planId !== 'free';
  }

  startTrial(planId: string) {
    if (this.startingTrial) return;
    this.startingTrial = planId;
    const plan = planId === 'pro' ? 'Pro' : 'Team';
    this.billing.startTrial(plan).subscribe({
      next: () => { this.startingTrial = null; },
      error: () => { this.startingTrial = null; },
    });
  }

  upgrade(priceId: string, planId: string) {
    if (this.upgrading) return;
    this.upgrading = planId;
    const successUrl = `${environment.publicUrl}/app/settings/billing?success=true`;
    const cancelUrl = `${environment.publicUrl}/app/settings/billing`;
    this.billing.createCheckout(priceId, successUrl, cancelUrl).subscribe({
      next: ({ url }) => window.location.href = url,
      error: () => { this.upgrading = null; },
    });
  }

  manageSubscription() {
    const returnUrl = `${environment.publicUrl}/app/settings/billing`;
    this.billing.createPortal(returnUrl).subscribe({
      next: ({ url }) => window.location.href = url,
    });
  }
}
