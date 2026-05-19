import { Injectable, signal, inject, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthResponse, AuthUser, LoginRequest, RegisterRequest } from '../models/auth.models';
import { LS_KEYS, API_PATHS, ERROR_MESSAGES } from '../constants';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);
  private baseUrl = `${environment.gatewayUrl}/gateway`;

  currentUser = signal<AuthUser | null>(this.getUserFromStorage());
  token = signal<string | null>(localStorage.getItem(LS_KEYS.TOKEN));
  isLicenseExpired = computed(() =>
    this.currentUser()?.subscriptionStatus === 'expired'
  );

  login(credentials: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.baseUrl}${API_PATHS.AUTH_LOGIN}`, credentials).pipe(
      tap(res => this.setSession(res))
    );
  }

  register(data: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.baseUrl}${API_PATHS.AUTH_REGISTER}`, data).pipe(
      tap(res => {
        if (res.token || res.accessToken) {
          this.setSession(res);
        }
      })
    );
  }

  verifyEmail(email: string, code: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.baseUrl}${API_PATHS.AUTH_VERIFY_EMAIL}`, { email, code }).pipe(
      tap(res => this.setSession(res))
    );
  }

  logout(): void {
    localStorage.removeItem(LS_KEYS.TOKEN);
    localStorage.removeItem(LS_KEYS.USER);
    localStorage.removeItem(LS_KEYS.TENANT);
    this.currentUser.set(null);
    this.token.set(null);
    this.router.navigate(['/auth/login']);
  }

  isAuthenticated(): boolean {
    const token = this.token();
    if (!token) return false;

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const isExpired = Date.now() >= payload.exp * 1000;
      return !isExpired;
    } catch {
      return false;
    }
  }

  private setSession(authRes: AuthResponse): void {
    const token = authRes.token ?? authRes.accessToken;
    const tenantId = authRes.tenantId ?? authRes.user?.tenantId ?? null;

    if (!token) {
      throw new Error(ERROR_MESSAGES['auth.token_missing']);
    }

    localStorage.setItem(LS_KEYS.TOKEN, token);
    if (authRes.user) {
      localStorage.setItem(LS_KEYS.USER, JSON.stringify(authRes.user));
      this.currentUser.set(authRes.user);
    } else {
      localStorage.removeItem(LS_KEYS.USER);
      this.currentUser.set(null);
    }

    if (tenantId) {
      localStorage.setItem(LS_KEYS.TENANT, tenantId);
    } else {
      localStorage.removeItem(LS_KEYS.TENANT);
    }

    this.token.set(token);
  }

  private getUserFromStorage(): AuthUser | null {
    const userStr = localStorage.getItem(LS_KEYS.USER);
    if (!userStr) return null;
    try { return JSON.parse(userStr); } catch { return null; }
  }
}
