import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of, throwError, Observable } from 'rxjs';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { vi } from 'vitest';

import { Login } from './login';
import { AuthService } from '../../../core/services/auth.service';

// ─── Helpers ──────────────────────────────────────────────────────────────────

function createAuthServiceMock() {
  return {
    login:       vi.fn(),
    currentUser: vi.fn().mockReturnValue({ role: 'Owner' }),
    token:       { set: vi.fn() },
  };
}

async function setupComponent(authServiceMock: ReturnType<typeof createAuthServiceMock>) {
  await TestBed.configureTestingModule({
    imports: [Login],
    providers: [
      provideHttpClient(),
      provideRouter([]),
      { provide: AuthService, useValue: authServiceMock },
    ],
  }).compileComponents();

  const router = TestBed.inject(Router);
  const navigateSpy = vi.spyOn(router, 'navigate').mockResolvedValue(true);

  const fixture: ComponentFixture<Login> = TestBed.createComponent(Login);
  const component = fixture.componentInstance;
  await fixture.whenStable();

  return { fixture, component, routerSpy: { navigate: navigateSpy } };
}

// ─── Tests ────────────────────────────────────────────────────────────────────

describe('Login Component', () => {
  let authMock: ReturnType<typeof createAuthServiceMock>;

  beforeEach(() => {
    authMock = createAuthServiceMock();
  });

  // TC-49
  it('should create the component', async () => {
    const { component } = await setupComponent(authMock);
    expect(component).toBeTruthy();
  });

  // TC-49
  it('TC-49: loginForm es inválido cuando email y password están vacíos', async () => {
    const { component } = await setupComponent(authMock);
    expect(component.loginForm.invalid).toBe(true);
  });

  // TC-50
  it('TC-50: campo email rechaza formato incorrecto', async () => {
    const { component } = await setupComponent(authMock);
    const emailCtrl = component.loginForm.controls.email;

    emailCtrl.setValue('no-es-un-email');
    expect(emailCtrl.hasError('email')).toBe(true);
  });

  it('TC-50b: campo email acepta formato correcto', async () => {
    const { component } = await setupComponent(authMock);
    const emailCtrl = component.loginForm.controls.email;

    emailCtrl.setValue('cajero@colmado.com');
    expect(emailCtrl.hasError('email')).toBe(false);
    expect(emailCtrl.valid).toBe(true);
  });

  // TC-51
  it('TC-51: onSubmit con form inválido no invoca AuthService.login', async () => {
    const { component } = await setupComponent(authMock);
    component.onSubmit();
    expect(authMock.login).not.toHaveBeenCalled();
  });

  // TC-52
  it('TC-52: onSubmit exitoso navega a /portal/dashboard', async () => {
    authMock.login.mockReturnValue(of({ token: 'jwt.token', user: { id: '1' } }));
    const { component, routerSpy } = await setupComponent(authMock);

    component.loginForm.setValue({ email: 'cajero@colmado.com', password: 'secret' });
    component.onSubmit();

    expect(authMock.login).toHaveBeenCalledWith({
      email: 'cajero@colmado.com',
      password: 'secret',
    });
    expect(routerSpy.navigate).toHaveBeenCalledWith(['/portal/dashboard']);
  });

  // TC-53
  it('TC-53: onSubmit en fallo establece error y loading=false', async () => {
    authMock.login.mockReturnValue(throwError(() => new Error('Unauthorized')));
    const { component } = await setupComponent(authMock);

    component.loginForm.setValue({ email: 'bad@x.com', password: 'wrong' });
    component.onSubmit();

    expect(component.error).toBe('Credenciales inválidas.');
    expect(component.loading).toBe(false);
  });

  it('TC-53b: loading es true mientras la petición está en curso', async () => {
    authMock.login.mockReturnValue(new Observable(() => {}));
    const { component } = await setupComponent(authMock);

    component.loginForm.setValue({ email: 'cajero@colmado.com', password: 'pass' });
    component.onSubmit();

    expect(component.loading).toBe(true);
    expect(component.error).toBeNull();
  });
});
