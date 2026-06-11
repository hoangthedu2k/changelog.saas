import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { map } from 'rxjs';
import { BillingService } from '../services/billing.service';

export const planGuard: CanActivateFn = () => {
  const billing = inject(BillingService);
  const router = inject(Router);

  const redirect = router.createUrlTree(['/app/settings/billing'], {
    queryParams: { upgrade: true },
  });

  const sub = billing.subscription();

  const isBlocked = (s: { plan: string; trialDaysLeft: number }) =>
    s.plan === 'Free' && s.trialDaysLeft === 0;

  if (sub !== null) {
    return isBlocked(sub) ? redirect : true;
  }

  return billing.loadSubscription().pipe(
    map(s => (isBlocked(s) ? redirect : true))
  );
};
