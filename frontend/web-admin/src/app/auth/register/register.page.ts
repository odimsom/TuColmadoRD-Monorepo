import { Component, ChangeDetectionStrategy, inject, signal, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth.service';
import { InputComponent } from '../../shared/ui/input/input.component';
import { BtnComponent } from '../../shared/ui/btn/btn.component';
import { TcLogoComponent } from '../../shared/ui/logo/tc-logo.component';

function passwordsMatch(ctrl: AbstractControl): ValidationErrors | null {
  const pw = ctrl.get('password')?.value;
  const confirm = ctrl.get('confirmPassword')?.value;
  return pw && confirm && pw !== confirm ? { passwordsMismatch: true } : null;
}

@Component({
  selector: 'app-register-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  imports: [ReactiveFormsModule, RouterLink, InputComponent, BtnComponent, TcLogoComponent],
  templateUrl: './register.page.html',
})
export class RegisterPage {
  private fb = inject(FormBuilder);
  private auth = inject(AuthService);
  private router = inject(Router);

  loading = signal(false);
  error = signal('');

  readonly perks = [
    { icon: 'lucide:calendar-check', text: 'Primer año completamente gratis' },
    { icon: 'lucide:headphones',     text: 'Soporte humano por WhatsApp' },
    { icon: 'lucide:megaphone',      text: 'Tu voz define las funciones' },
    { icon: 'lucide:shield-check',   text: 'Sin tarjeta de crédito' },
  ];

  form = this.fb.nonNullable.group({
    tenantName: ['', [Validators.required, Validators.minLength(2)]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
    confirmPassword: ['', Validators.required],
  }, { validators: passwordsMatch });

  get tenantError(): string {
    const c = this.form.controls.tenantName;
    if (c.touched && c.errors?.['required']) return 'El nombre del negocio es requerido';
    if (c.touched && c.errors?.['minlength']) return 'Mínimo 2 caracteres';
    return '';
  }

  get emailError(): string {
    const c = this.form.controls.email;
    if (c.touched && c.errors?.['required']) return 'El correo es requerido';
    if (c.touched && c.errors?.['email']) return 'Correo no válido';
    return '';
  }

  get passwordError(): string {
    const c = this.form.controls.password;
    if (c.touched && c.errors?.['required']) return 'La contraseña es requerida';
    if (c.touched && c.errors?.['minlength']) return 'Mínimo 8 caracteres';
    return '';
  }

  get confirmError(): string {
    const c = this.form.controls.confirmPassword;
    if (c.touched && this.form.errors?.['passwordsMismatch']) return 'Las contraseñas no coinciden';
    return '';
  }

  submit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid || this.loading()) return;

    this.loading.set(true);
    this.error.set('');

    const { tenantName, email, password } = this.form.getRawValue();

    this.auth.register({ tenantName, email, password }).subscribe({
      next: () => this.router.navigate(['/auth/verify'], { queryParams: { email } }),
      error: (e: { error?: { message?: string }; message?: string }) => {
        this.error.set(e.error?.message ?? e.message ?? 'Error al crear la cuenta');
        this.loading.set(false);
      },
    });
  }
}
