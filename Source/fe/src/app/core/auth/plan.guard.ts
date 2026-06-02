import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

const PLAN_RANK: Record<string, number> = { free: 0, pro: 1, team: 2 };

export const planGuard = (required: 'pro' | 'team'): CanActivateFn => () => {
  const user = inject(AuthService).currentUser();
  if (!user) return inject(Router).createUrlTree(['/login']);

  return (PLAN_RANK[user.plan===undefined ? 'free' : user.plan] ?? 0) >= PLAN_RANK[required]
    || inject(Router).createUrlTree(['/upgrade']);
};
