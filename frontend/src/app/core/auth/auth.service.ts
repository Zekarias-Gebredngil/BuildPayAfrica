import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { BehaviorSubject, map, tap } from 'rxjs';
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

  private readonly state$ = new BehaviorSubject<AuthState>(this.readInitialState());

  constructor(
    private readonly http:   HttpClient,
    private readonly router: Router
  ) {}

  authState$ = this.state$.asObservable();

  get token(): string | null        { return this.state$.value.token; }
  get roles(): string[]             { return this.state$.value.roles; }
  get displayName(): string         {
    const p = this.state$.value.payload;
    return p?.name ?? p?.preferred_username ?? p?.email ?? '';
  }

  get isAuthenticated(): boolean {
    const { token, payload } = this.state$.value;
    if (!token || !payload?.exp) return false;
    return payload.exp * 1000 > Date.now() + 30_000;
  }

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
    this.state$.next({ token: null, refreshToken: null, payload: null, roles: [] });
    this.router.navigateByUrl('/login');
  }

  /** Called by the auth interceptor when a 401 is received. */
  handleUnauthorized() {
    this.logout();
  }

  private setTokens(token: string, refreshToken?: string) {
    localStorage.setItem(this.tokenKey, token);
    if (refreshToken) localStorage.setItem(this.refreshKey, refreshToken);
    const payload = decodeJwt(token);
    this.state$.next({
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
