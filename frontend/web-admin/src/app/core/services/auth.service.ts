import { Injectable, signal, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface AuthUser {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  tenantId: string;
}

export interface AuthResponse {
  token?: string;
  accessToken?: string;
  tenantId?: string;
  user: AuthUser;
}

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

  login(credentials: any): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.baseUrl}/auth/login`, credentials).pipe(
      tap(res => this.setSession(res))
    );
  }

  register(data: any): Observable<AuthResponse> {
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
    const tenantId = authRes.tenantId ?? authRes.user?.tenantId;

    if (!token) {
      throw new Error('AUTH_TOKEN_MISSING');
    }

    localStorage.setItem('tc_token', token);
    localStorage.setItem('tc_user', JSON.stringify(authRes.user));
    if (tenantId) {
      localStorage.setItem('tc_tenant', tenantId);
    }
    this.currentUser.set(authRes.user);
    this.token.set(token);
  }

  private getUserFromStorage(): AuthUser | null {
    const userStr = localStorage.getItem('tc_user');
    return userStr ? JSON.parse(userStr) : null;
  }
}
