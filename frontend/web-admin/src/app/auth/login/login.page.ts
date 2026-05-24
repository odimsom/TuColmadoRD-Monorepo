import { Component, ChangeDetectionStrategy, inject, signal, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth.service';
import { ToastService } from '../../shared/ui/toast/toast.service';
import { InputComponent } from '../../shared/ui/input/input.component';
import { BtnComponent } from '../../shared/ui/btn/btn.component';
import { TcLogoComponent } from '../../shared/ui/logo/tc-logo.component';

@Component({
  selector: 'app-login-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  imports: [ReactiveFormsModule, RouterLink, InputComponent, BtnComponent, TcLogoComponent],
  templateUrl: './login.page.html',
})
export class LoginPage {
  private fb = inject(FormBuilder);
  private auth = inject(AuthService);
  private router = inject(Router);
  private toast = inject(ToastService);

  loading = signal(false);
  error = signal('');

  readonly features = [
    { icon: 'lucide:package',       text: 'Inventario en tiempo real' },
    { icon: 'lucide:scan-barcode',  text: 'Punto de venta rápido' },
    { icon: 'lucide:book-open',     text: 'Control de fiados' },
    { icon: 'lucide:bar-chart-2',   text: 'Reportes que entienden tu negocio' },
  ];

  form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
  });

  get emailError(): string {
    const c = this.form.controls.email;
    if (c.touched && c.errors?.['required']) return 'El correo es requerido';
    if (c.touched && c.errors?.['email']) return 'Correo no válido';
    return '';
  }

  get passwordError(): string {
    const c = this.form.controls.password;
    if (c.touched && c.errors?.['required']) return 'La contraseña es requerida';
    return '';
  }

  submit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid || this.loading()) return;

    this.loading.set(true);
    this.error.set('');

    this.auth.login(this.form.getRawValue()).subscribe({
      next: () => this.router.navigate(['/portal/dashboard']),
      error: (e: { error?: { message?: string }; message?: string }) => {
        this.error.set(e.error?.message ?? e.message ?? 'Error al iniciar sesión');
        this.loading.set(false);
      },
    });
  }
}
