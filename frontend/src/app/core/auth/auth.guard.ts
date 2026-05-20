import { CanActivateFn, Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from './auth.service';

export const authGuard: CanActivateFn = (
  _route: ActivatedRouteSnapshot,
  state:  RouterStateSnapshot
) => {
  const auth   = inject(AuthService);
  const router = inject(Router);

  if (auth.isAuthenticated) return true;

  // Preserve the attempted URL so we can redirect back after login
  return router.createUrlTree(['/login'], {
    queryParams: { returnUrl: state.url }
  });
};
