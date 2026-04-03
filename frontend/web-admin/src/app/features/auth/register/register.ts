import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { DownloadService, DownloadInfo } from '../../../core/services/download.service';
import { environment } from '../../../../environments/environment';

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

  registerForm = this.fb.group({
    fullName: ['', [Validators.required, Validators.minLength(3)]],
    businessName: ['', [Validators.required, Validators.minLength(3)]],
    email: ['', [Validators.required, Validators.email]],
    whatsapp: ['', [Validators.required, Validators.pattern(/^\+?[1-9]\d{1,14}$/)]],
    password: ['', [Validators.required, Validators.minLength(6)]]
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

    this.state.set('loading');
    this.error.set(null);

    const payload = {
      tenantName: this.registerForm.value.businessName?.trim() ?? '',
      email: this.registerForm.value.email?.trim().toLowerCase() ?? '',
      password: this.registerForm.value.password ?? '',
    };

    this.authService.register(payload).subscribe({
      next: () => {
        this.state.set('success');
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
