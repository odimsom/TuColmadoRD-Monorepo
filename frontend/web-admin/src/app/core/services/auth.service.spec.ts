import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { provideRouter } from '@angular/router';
import { vi } from 'vitest';

import { AuthService } from './auth.service';

// ─── Helpers ──────────────────────────────────────────────────────────────────

/** Genera un JWT con la estructura base64(header).base64(payload).signature */
function makeJwt(payload: Record<string, unknown>): string {
  const header = btoa(JSON.stringify({ alg: 'HS256', typ: 'JWT' }));
  const body = btoa(JSON.stringify(payload));
  return `${header}.${body}.fake-signature`;
}

function futureExp(secondsFromNow = 3600): number {
  return Math.floor(Date.now() / 1000) + secondsFromNow;
}

function pastExp(secondsAgo = 3600): number {
  return Math.floor(Date.now() / 1000) - secondsAgo;
}

// ─── Tests ────────────────────────────────────────────────────────────────────

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;
  let routerSpy: { navigate: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    localStorage.clear();
    routerSpy = { navigate: vi.fn() };

    TestBed.configureTestingModule({
      providers: [
        AuthService,
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        { provide: Router, useValue: routerSpy },
      ],
    });

    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
    localStorage.clear();
  });

  // TC-54
  it('TC-54: isAuthenticated retorna false cuando no hay token', () => {
    expect(service.isAuthenticated()).toBe(false);
  });

  // TC-55
  it('TC-55: isAuthenticated retorna false con token JWT expirado', () => {
    const expiredToken = makeJwt({ sub: '1', exp: pastExp() });
    localStorage.setItem('tc_token', expiredToken);

    // Re-create service so signal picks up the stored token
    service['token'].set(expiredToken);

    expect(service.isAuthenticated()).toBe(false);
  });

  it('TC-55b: isAuthenticated retorna true con token JWT válido', () => {
    const validToken = makeJwt({ sub: '1', exp: futureExp() });
    service['token'].set(validToken);

    expect(service.isAuthenticated()).toBe(true);
  });

  // TC-56
  it('TC-56: logout limpia localStorage y pone currentUser=null', () => {
    localStorage.setItem('tc_token', 'some-token');
    localStorage.setItem('tc_user', JSON.stringify({ id: '1', email: 'x@x.com' }));
    localStorage.setItem('tc_tenant', 'tenant-001');

    service.logout();

    expect(localStorage.getItem('tc_token')).toBeNull();
    expect(localStorage.getItem('tc_user')).toBeNull();
    expect(localStorage.getItem('tc_tenant')).toBeNull();
    expect(service.currentUser()).toBeNull();
    expect(routerSpy.navigate).toHaveBeenCalledWith(['/auth/login']);
  });

  it('TC-56b: logout redirige a /auth/login', () => {
    service.logout();
    expect(routerSpy.navigate).toHaveBeenCalledWith(['/auth/login']);
  });

  it('isAuthenticated retorna false con payload JWT malformado', () => {
    service['token'].set('not.a.valid.jwt.at.all');
    expect(service.isAuthenticated()).toBe(false);
  });
});
