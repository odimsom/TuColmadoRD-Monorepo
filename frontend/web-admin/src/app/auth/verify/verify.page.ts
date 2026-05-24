import { Component, ChangeDetectionStrategy, inject, signal, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../core/auth.service';
import { InputComponent } from '../../shared/ui/input/input.component';
import { BtnComponent } from '../../shared/ui/btn/btn.component';
import { TcLogoComponent } from '../../shared/ui/logo/tc-logo.component';

@Component({
  selector: 'app-verify-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  imports: [ReactiveFormsModule, InputComponent, BtnComponent, TcLogoComponent],
  template: `
    <div class="min-h-screen bg-base-100 flex items-center justify-center px-4">
      <div class="w-full max-w-sm">

        <div class="mb-10">
          <app-tc-logo size="sm" />
        </div>

        <div class="w-12 h-12 bg-primary/10 flex items-center justify-center mb-6">
          <iconify-icon icon="lucide:mail" class="text-primary text-2xl"></iconify-icon>
        </div>

        <h1 class="text-2xl font-black text-base-content tracking-tight mb-2">Verifica tu correo</h1>
        <p class="text-base-content/50 text-sm mb-1">
          Enviamos un código de verificación a
        </p>
        <p class="text-primary font-semibold text-sm mb-8">{{ email() }}</p>

        <form [formGroup]="form" (ngSubmit)="submit()" novalidate class="space-y-5">
          <app-input
            formControlName="code"
            label="Código de verificación"
            type="text"
            placeholder="000000"
            autocomplete="one-time-code"
            [required]="true"
            [error]="codeError"
          />

          @if (error()) {
            <div role="alert" class="alert alert-error text-sm py-2">
              <span>{{ error() }}</span>
            </div>
          }

          <button
            appBtn
            type="submit"
            variant="primary"
            [wide]="true"
            [loading]="loading()"
          >
            Verificar cuenta
          </button>
        </form>

        <p class="text-center text-xs text-base-content/30 mt-6">
          ¿No recibiste el código?
          <button class="text-primary hover:underline" type="button">Reenviar</button>
        </p>
      </div>
    </div>
  `,
})
export class VerifyPage {
  private fb = inject(FormBuilder);
  private auth = inject(AuthService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  loading = signal(false);
  error = signal('');
  email = signal(this.route.snapshot.queryParamMap.get('email') ?? '');

  form = this.fb.nonNullable.group({
    code: ['', [Validators.required, Validators.minLength(4)]],
  });

  get codeError(): string {
    const c = this.form.controls.code;
    if (c.touched && c.errors?.['required']) return 'El código es requerido';
    return '';
  }

  submit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid || this.loading()) return;

    this.loading.set(true);
    this.error.set('');

    this.auth.verifyEmail(this.email(), this.form.controls.code.value).subscribe({
      next: () => this.router.navigate(['/portal/dashboard']),
      error: (e: { error?: { message?: string }; message?: string }) => {
        this.error.set(e.error?.message ?? 'Código inválido');
        this.loading.set(false);
      },
    });
  }
}
