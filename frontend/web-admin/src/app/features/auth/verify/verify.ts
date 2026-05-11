import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-verify',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './verify.html',
  styleUrl: './verify.scss'
})
export class Verify implements OnInit {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  email = signal<string>('');
  loading = signal(false);
  error = signal<string | null>(null);

  verifyForm = this.fb.nonNullable.group({
    code: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(6)]]
  });

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      if (params['email']) {
        this.email.set(params['email']);
      } else {
        this.router.navigate(['/auth/login']);
      }
    });
  }

  onSubmit(): void {
    if (this.verifyForm.invalid || !this.email()) {
      this.verifyForm.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.error.set(null);

    const code = this.verifyForm.controls.code.value.trim();

    this.authService.verifyEmail(this.email(), code).subscribe({
      next: () => {
        this.loading.set(false);
        this.router.navigate(['/portal/dashboard']).catch(err => {
          this.error.set('Error al redirigir al portal.');
        });
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err.error?.message || 'Código inválido o expirado. Inténtalo de nuevo.');
      }
    });
  }
}
