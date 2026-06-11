import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { map, of } from 'rxjs';
import { BillingService } from '../services/billing.service';

export const planGuard: CanActivateFn = () => {
  const billing = inject(BillingService);
  const router = inject(Router);

  const redirect = router.createUrlTree(['/app/settings/billing'], {
    queryParams: { upgrade: true },
  });

  const sub = billing.subscription();

  // Already loaded — check synchronously
  if (sub !== null) {
    return sub.plan === 'Free' ? redirect : true;
  }

  // Not loaded yet — fetch then check
  return billing.loadSubscription().pipe(
    map(s => (s.plan === 'Free' ? redirect : true))
  );
};
