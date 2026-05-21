import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from './auth.service';

/**
 * Prevents authenticated users from accessing login/register pages.
 * Redirects them to their role dashboard if already signed in.
 */
export const guestGuard: CanActivateFn = () => {
  const auth   = inject(AuthService);
  const router = inject(Router);

  // If a stale/bad token is present, clear it so the user can access auth pages cleanly.
  if (auth.token && !auth.isAuthenticated) {
    auth.clearSession();
    return true;
  }

  if (!auth.isAuthenticated) return true;

  return router.parseUrl(auth.roleHomePath);
};
