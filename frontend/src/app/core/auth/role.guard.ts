import { CanActivateFn, ActivatedRouteSnapshot, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from './auth.service';

export const roleGuard: CanActivateFn = (route: ActivatedRouteSnapshot) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  const allowed = (route.data['roles'] as string[] | undefined) ?? [];
  if (allowed.length === 0) return true;

  if (!auth.isAuthenticated) return router.createUrlTree(['/login']);

  const hasRole = allowed.some((r) => auth.roles.includes(r));
  return hasRole ? true : router.createUrlTree(['/']);
};

