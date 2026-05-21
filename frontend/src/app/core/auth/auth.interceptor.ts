import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from './auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  if (!req.url.startsWith('/api') && !req.url.startsWith('/uploads')) {
    return next(req);
  }

  const auth = inject(AuthService);

  return auth.ensureFreshToken().pipe(
    switchMap((token) => {
      const authReq = token ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : req;
      return next(authReq);
    }),
    catchError((err: HttpErrorResponse) => {
      if (err.status !== 401) return throwError(() => err);

      return auth.refresh().pipe(
        switchMap((ok) => {
          if (!ok || !auth.token) {
            auth.handleUnauthorized();
            return throwError(() => err);
          }

          const retryReq = req.clone({ setHeaders: { Authorization: `Bearer ${auth.token}` } });
          return next(retryReq);
        })
      );
    })
  );
};

