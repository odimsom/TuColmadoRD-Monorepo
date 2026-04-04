import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { DownloadService, DownloadInfo } from '../../../core/services/download.service';
import { RegisterRequest } from '../../../core/models/auth.models';

type RegisterState = 'form' | 'terms-modal' | 'loading' | 'success' | 'error';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './register.html',
  styleUrl: './register.scss'
})
export class Register implements OnInit {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private downloadService = inject(DownloadService);
  private router = inject(Router);

  state = signal<RegisterState>('form');
  error = signal<string | null>(null);
  termsAccepted = signal(false);
  downloadInfo = signal<DownloadInfo | null>(null);

  registerForm = this.fb.nonNullable.group({
    businessName: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(120)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(160)]],
    password: ['', [Validators.required, Validators.minLength(8), Validators.maxLength(128)]]
  });

  ngOnInit(): void {
    this.downloadService.getLatestTestRelease().subscribe(info => {
      this.downloadInfo.set(info);
    });
  }

  onTrySubmit(): void {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }
    this.state.set('terms-modal');
  }

  cancelTerms(): void {
    this.state.set('form');
  }

  confirmRegistration(): void {
    if (!this.termsAccepted()) return;
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      this.state.set('form');
      return;
    }

    this.state.set('loading');
    this.error.set(null);

    const payload: RegisterRequest = {
      tenantName: this.registerForm.controls.businessName.value.trim(),
      email: this.registerForm.controls.email.value.trim().toLowerCase(),
      password: this.registerForm.controls.password.value,
    };

    this.authService.register(payload).subscribe({
      next: () => {
        this.router.navigate(['/portal/welcome']);
      },
      error: (err) => {
        this.state.set('error');
        this.error.set(err.error?.message || 'Error al procesar el registro. Inténtalo de nuevo.');
        console.error('Register error:', err);
      }
    });
  }

  retry(): void {
    this.state.set('form');
  }
}
