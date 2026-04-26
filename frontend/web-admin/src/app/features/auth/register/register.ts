import { Component, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { RegisterRequest } from '../../../core/models/auth.models';

type RegisterStep = 'account' | 'plan' | 'checkout';
type BillingCycle = 'monthly' | 'annual';

interface Plan {
  id: string;
  name: string;
  priceMonthly: number;
  priceAnnual: number;
  features: string[];
}

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './register.html',
  styleUrl: './register.scss'
})
export class Register {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  step = signal<RegisterStep>('account');
  billingCycle = signal<BillingCycle>('monthly');
  selectedPlanId = signal<string | null>(null);
  loading = signal(false);
  error = signal<string | null>(null);

  plans: Plan[] = [
    {
      id: 'basic',
      name: 'Básico',
      priceMonthly: 800,
      priceAnnual: 7200,
      features: ['Facturación ilimitada', 'Control de inventario', 'Cierre de caja diario', 'Soporte por WhatsApp'],
    },
    {
      id: 'pro',
      name: 'Pro',
      priceMonthly: 1500,
      priceAnnual: 13500,
      features: ['Todo lo del Básico', 'Múltiples cajeros', 'Reportes avanzados', 'Soporte prioritario 24/7', 'Fiados y crédito'],
    },
  ];

  selectedPlan = computed(() => this.plans.find(p => p.id === this.selectedPlanId()) ?? null);

  registerForm = this.fb.nonNullable.group({
    businessName: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(120)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(160)]],
    password: ['', [Validators.required, Validators.minLength(8), Validators.maxLength(128)]],
  });

  stepLabel = computed(() => {
    switch (this.step()) {
      case 'account':  return '1 de 3 — Crear cuenta';
      case 'plan':     return '2 de 3 — Elegir plan';
      case 'checkout': return '3 de 3 — Pago';
    }
  });

  submitAccount(): void {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }
    this.error.set(null);
    this.step.set('plan');
  }

  selectPlan(planId: string): void {
    this.selectedPlanId.set(planId);
  }

  confirmPlan(): void {
    if (!this.selectedPlanId()) return;
    this.step.set('checkout');
  }

  toggleBilling(): void {
    this.billingCycle.update(v => v === 'monthly' ? 'annual' : 'monthly');
  }

  currentPrice(plan: Plan): number {
    return this.billingCycle() === 'monthly' ? plan.priceMonthly : plan.priceAnnual;
  }

  back(): void {
    if (this.step() === 'plan')     this.step.set('account');
    if (this.step() === 'checkout') this.step.set('plan');
  }

  submitPayment(): void {
    if (this.registerForm.invalid || !this.selectedPlanId()) return;

    this.loading.set(true);
    this.error.set(null);

    const payload: RegisterRequest = {
      tenantName: this.registerForm.controls.businessName.value.trim(),
      email: this.registerForm.controls.email.value.trim().toLowerCase(),
      password: this.registerForm.controls.password.value,
    };

    this.authService.register(payload).subscribe({
      next: () => this.router.navigate(['/portal/dashboard']),
      error: (err) => {
        this.loading.set(false);
        this.error.set(err.error?.message || 'Error al crear la cuenta. Inténtalo de nuevo.');
      },
    });
  }
}
