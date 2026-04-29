import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';

interface Plan {
  id: string;
  name: string;
  priceMonthly: number;
  priceAnnual: number;
  features: string[];
  highlighted: boolean;
}

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './home.html',
  styleUrl: './home.scss',
})
export class Home {
  private router = inject(Router);

  billingCycle = signal<'monthly' | 'annual'>('monthly');

  plans: Plan[] = [
    {
      id: 'basic',
      name: 'Básico',
      priceMonthly: 800,
      priceAnnual: 7200,
      features: [
        'Facturación ilimitada',
        'Control de inventario',
        'Cierre de caja diario',
        'Soporte por WhatsApp',
      ],
      highlighted: false,
    },
    {
      id: 'pro',
      name: 'Pro',
      priceMonthly: 1500,
      priceAnnual: 13500,
      features: [
        'Todo lo del Básico',
        'Múltiples cajeros',
        'Reportes avanzados',
        'Soporte prioritario 24/7',
        'Fiados y crédito a clientes',
      ],
      highlighted: true,
    },
  ];

  currentPrice(plan: Plan): number {
    return this.billingCycle() === 'monthly' ? plan.priceMonthly : plan.priceAnnual;
  }

  monthlyEquivalent(plan: Plan): number {
    return Math.round(plan.priceAnnual / 12);
  }

  toggleBilling() {
    this.billingCycle.update(v => v === 'monthly' ? 'annual' : 'monthly');
  }

  goToRegister() {
    this.router.navigate(['/auth/register']);
  }

  goToLogin() {
    this.router.navigate(['/auth/login']);
  }
}
