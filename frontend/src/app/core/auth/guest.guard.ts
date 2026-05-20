import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from './auth.service';

/**
 * Prevents authenticated users from accessing login/register pages.
 * Redirects them to home if they're already signed in.
 */
export const guestGuard: CanActivateFn = () => {
  const auth   = inject(AuthService);
  const router = inject(Router);

  if (!auth.isAuthenticated) return true;

  return router.createUrlTree(['/']);
};
