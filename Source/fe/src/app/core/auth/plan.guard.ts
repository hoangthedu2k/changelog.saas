import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { map } from 'rxjs';
import { BillingService } from '../services/billing.service';

export const planGuard: CanActivateFn = () => {
  const billing = inject(BillingService);
  const router = inject(Router);
  const redirect = router.createUrlTree(['/app/settings/billing'], { queryParams: { upgrade: true } });

  const sub = billing.subscription();
  if (sub !== null) return sub.plan === 'Free' ? redirect : true;

  return billing.loadSubscription().pipe(
    map(s => s.plan === 'Free' ? redirect : true)
  );
};
