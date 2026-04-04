import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { isDesktopApp } from '../../../core/utils/runtime';
import { LoginRequest } from '../../../core/models/auth.models';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './login.html',
  styleUrl: './login.scss',
})
export class Login {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  loginForm = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]]
  });

  error: string | null = null;
  loading = false;
  readonly desktopApp = isDesktopApp();

  onSubmit(): void {
    if (this.loginForm.invalid) return;

    this.loading = true;
    this.error = null;

    const payload: LoginRequest = {
      email: this.loginForm.controls.email.value?.trim().toLowerCase() ?? '',
      password: this.loginForm.controls.password.value ?? ''
    };

    this.authService.login(payload).subscribe({
      next: () => {
        this.router.navigate(['/portal/dashboard']);
      },
      error: (err) => {
        this.loading = false;
        this.error = 'Credenciales inválidas.';
        console.error('Login error:', err);
      }
    });
  }
}
