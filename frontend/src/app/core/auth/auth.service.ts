import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { map, tap, catchError, Observable, of, shareReplay, finalize } from 'rxjs';
import { decodeJwt, getJwtRoles, JwtPayload } from './jwt';

export type AuthState = {
  token:        string | null;
  refreshToken: string | null;
  payload:      JwtPayload | null;
  roles:        string[];
};

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly tokenKey   = 'buildpay_token';
  private readonly refreshKey = 'buildpay_refresh';

  // ── Signal-based state — reactive to computed() in components ────────────
  private readonly _state = signal<AuthState>(this.readInitialState());

  // Public read-only signals — components use these directly
  readonly authState   = this._state.asReadonly();
  readonly token$      = computed(() => this._state().token);
  readonly roles$      = computed(() => this._state().roles);
  readonly displayName$ = computed(() => {
    const p = this._state().payload;
    return p?.name ?? p?.preferred_username ?? p?.email ?? '';
  });
  readonly isAuthenticated$ = computed(() => {
    const { token, payload } = this._state();
    if (!token || !payload?.exp) return false;
    return payload.exp * 1000 > Date.now() + 30_000;
  });

  // Keep plain getters for non-reactive consumers (interceptors, guards)
  get token(): string | null  { return this._state().token; }
  get roles(): string[]       { return this._state().roles; }
  get displayName(): string   { return this.displayName$(); }
  get isAuthenticated(): boolean { return this.isAuthenticated$(); }

  private refreshInFlight$: Observable<boolean> | null = null;

  constructor(
    private readonly http:   HttpClient,
    private readonly router: Router
  ) {}

  login(email: string, password: string) {
    return this.http
      .post<{ token: string; refreshToken: string; expiresIn: number }>(
        `/api/auth/login`,
        { email, password }
      )
      .pipe(
        tap((res) => this.setTokens(res.token, res.refreshToken)),
        map(() => true)
      );
  }

  register(fullName: string, email: string, password: string, role: string) {
    return this.http.post(`/api/auth/register`, { fullName, email, password, role });
  }

  logout() {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.refreshKey);
    this._state.set({ token: null, refreshToken: null, payload: null, roles: [] });
    this.router.navigateByUrl('/login');
  }

  handleUnauthorized() {
    this.logout();
  }

  refresh(): Observable<boolean> {
    const refreshToken = this._state().refreshToken;
    if (!refreshToken) return of(false);

    if (this.refreshInFlight$) return this.refreshInFlight$;

    this.refreshInFlight$ = this.http
      .post<{ token: string; refreshToken: string; expiresIn: number }>(
        `/api/auth/refresh`,
        { refreshToken }
      )
      .pipe(
        tap((res) => this.setTokens(res.token, res.refreshToken)),
        map(() => true),
        catchError(() => of(false)),
        finalize(() => (this.refreshInFlight$ = null)),
        shareReplay(1)
      );

    return this.refreshInFlight$;
  }

  ensureFreshToken(): Observable<string | null> {
    if (!this.token) return of(null);
    if (this.isAuthenticated) return of(this.token);
    return this.refresh().pipe(map((ok) => (ok ? this.token : null)));
  }

  private setTokens(token: string, refreshToken?: string) {
    localStorage.setItem(this.tokenKey, token);
    if (refreshToken) localStorage.setItem(this.refreshKey, refreshToken);
    const payload = decodeJwt(token);
    // Signal update — triggers all computed() that depend on authState
    this._state.set({
      token,
      refreshToken: refreshToken ?? localStorage.getItem(this.refreshKey),
      payload,
      roles: getJwtRoles(payload)
    });
  }

  private readInitialState(): AuthState {
    const token        = localStorage.getItem(this.tokenKey);
    const refreshToken = localStorage.getItem(this.refreshKey);
    if (!token) return { token: null, refreshToken: null, payload: null, roles: [] };
    const payload = decodeJwt(token);
    return { token, refreshToken, payload, roles: getJwtRoles(payload) };
  }
}
