import { Injectable, signal, inject, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { AuthUser, LoginRequest, RegisterRequest, AuthResponse } from './models/auth.model';

const LS_TOKEN = 'tc_token';
const LS_USER = 'tc_user';
const LS_TENANT = 'tc_tenant';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);
  private api = `${environment.gatewayUrl}/gateway`;

  readonly currentUser = signal<AuthUser | null>(this.restoreUser());
  readonly token = signal<string | null>(localStorage.getItem(LS_TOKEN));
  readonly isLicenseExpired = computed(() =>
    this.currentUser()?.subscriptionStatus === 'expired'
  );

  login(body: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.api}/auth/login`, body).pipe(
      tap(r => this.persist(r))
    );
  }

  register(body: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.api}/auth/register`, body).pipe(
      tap(r => { if (r.token || r.accessToken) this.persist(r); })
    );
  }

  verifyEmail(email: string, code: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.api}/auth/verify-email`, { email, code }).pipe(
      tap(r => this.persist(r))
    );
  }

  logout(): void {
    [LS_TOKEN, LS_USER, LS_TENANT].forEach(k => localStorage.removeItem(k));
    this.currentUser.set(null);
    this.token.set(null);
    this.router.navigate(['/auth/login']);
  }

  isAuthenticated(): boolean {
    const t = this.token();
    if (!t) return false;
    try {
      const payload = JSON.parse(atob(t.split('.')[1])) as { exp: number };
      return Date.now() < payload.exp * 1000;
    } catch {
      return false;
    }
  }

  private persist(r: AuthResponse): void {
    const token = r.token ?? r.accessToken;
    if (!token) return;
    localStorage.setItem(LS_TOKEN, token);
    this.token.set(token);
    if (r.user) {
      localStorage.setItem(LS_USER, JSON.stringify(r.user));
      this.currentUser.set(r.user);
    }
    const tenantId = r.tenantId ?? r.user?.tenantId;
    if (tenantId) localStorage.setItem(LS_TENANT, tenantId);
  }

  private restoreUser(): AuthUser | null {
    try {
      return JSON.parse(localStorage.getItem(LS_USER) ?? 'null') as AuthUser | null;
    } catch {
      return null;
    }
  }
}
