import { Injectable, signal, inject, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthResponse, AuthUser, LoginRequest, RegisterRequest } from '../models/auth.models';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);
  private baseUrl = `${environment.gatewayUrl}/gateway`;
  
  // State
  currentUser = signal<AuthUser | null>(this.getUserFromStorage());
  token = signal<string | null>(localStorage.getItem('tc_token'));
  isLicenseExpired = computed(() =>
    this.currentUser()?.subscriptionStatus === 'expired'
  );

  login(credentials: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.baseUrl}/auth/login`, credentials).pipe(
      tap(res => this.setSession(res))
    );
  }

  register(data: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.baseUrl}/auth/register`, data).pipe(
      tap(res => this.setSession(res))
    );
  }

  logout(): void {
    localStorage.removeItem('tc_token');
    localStorage.removeItem('tc_user');
    localStorage.removeItem('tc_tenant');
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
      throw new Error('AUTH_TOKEN_MISSING');
    }

    localStorage.setItem('tc_token', token);
    if (authRes.user) {
      localStorage.setItem('tc_user', JSON.stringify(authRes.user));
      this.currentUser.set(authRes.user);
    } else {
      localStorage.removeItem('tc_user');
      this.currentUser.set(null);
    }

    if (tenantId) {
      localStorage.setItem('tc_tenant', tenantId);
    } else {
      localStorage.removeItem('tc_tenant');
    }

    this.token.set(token);
  }

  private getUserFromStorage(): AuthUser | null {
    const userStr = localStorage.getItem('tc_user');
    if (!userStr) return null;
    try { return JSON.parse(userStr); } catch { return null; }
  }
}
