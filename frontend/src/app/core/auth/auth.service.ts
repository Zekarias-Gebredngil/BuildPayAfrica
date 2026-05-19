import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, map, tap } from 'rxjs';
import { decodeJwt, getJwtRoles, JwtPayload } from './jwt';

export type AuthState = {
  token: string | null;
  payload: JwtPayload | null;
  roles: string[];
};

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly storageKey = 'buildpay_token';
  private readonly state$ = new BehaviorSubject<AuthState>(this.readInitialState());

  constructor(private readonly http: HttpClient) {}

  authState$ = this.state$.asObservable();

  get token(): string | null {
    return this.state$.value.token;
  }

  get roles(): string[] {
    return this.state$.value.roles;
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
        tap((res) => this.setToken(res.token)),
        map(() => true)
      );
  }

  register(fullName: string, email: string, password: string, role: string) {
    return this.http.post(`/api/auth/register`, { fullName, email, password, role });
  }

  logout() {
    localStorage.removeItem(this.storageKey);
    this.state$.next({ token: null, payload: null, roles: [] });
  }

  private setToken(token: string) {
    localStorage.setItem(this.storageKey, token);
    const payload = decodeJwt(token);
    this.state$.next({ token, payload, roles: getJwtRoles(payload) });
  }

  private readInitialState(): AuthState {
    const token = localStorage.getItem(this.storageKey);
    if (!token) return { token: null, payload: null, roles: [] };
    const payload = decodeJwt(token);
    return { token, payload, roles: getJwtRoles(payload) };
  }
}

