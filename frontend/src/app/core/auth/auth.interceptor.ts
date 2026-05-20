import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { AuthService } from './auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  // Only attach the token to requests going to our own API
  if (!req.url.startsWith('/api') && !req.url.startsWith('/uploads')) {
    return next(req);
  }

  const auth  = inject(AuthService);
  const token = auth.token;

  const authReq = token
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(authReq).pipe(
    catchError((err: HttpErrorResponse) => {
      if (err.status === 401 && auth.isAuthenticated) {
        // Token was rejected by the server — clear session and redirect to login
        auth.handleUnauthorized();
      }
      return throwError(() => err);
    })
  );
};
