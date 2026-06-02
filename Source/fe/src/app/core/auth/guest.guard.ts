import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

export const guestGuard: CanActivateFn = () => {
  if (inject(AuthService).isLoggedIn()) {
    return inject(Router).createUrlTree(['/app']);
  }
  return true;
};
