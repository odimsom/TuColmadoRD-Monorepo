import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../core/services/auth.service';

interface Plan {
  id: string;
  name: string;
  monthlyPrice: number;
  yearlyPrice: number;
  highlighted: boolean;
  features: string[];
}

@Component({
  selector: 'app-subscription',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './subscription.html',
  styleUrl: './subscription.scss',
})
export class Subscription {
  private authService = inject(AuthService);

  user = this.authService.currentUser;
  isExpired = this.authService.isLicenseExpired;

  readonly plans: Plan[] = [
    {
      id: 'basico',
      name: 'Básico',
      monthlyPrice: 800,
      yearlyPrice: 7200,
      highlighted: false,
      features: [
        '1 usuario',
        'Inventario ilimitado',
        'Ventas y turnos',
        'Reportes básicos',
        'Soporte por WhatsApp',
      ],
    },
    {
      id: 'pro',
      name: 'Pro',
      monthlyPrice: 1500,
      yearlyPrice: 13500,
      highlighted: true,
      features: [
        'Usuarios ilimitados',
        'Todo lo de Básico',
        'App de escritorio (.exe)',
        'Fiados y créditos',
        'Reportes avanzados',
        'Soporte prioritario',
      ],
    },
  ];

  readonly whatsappUrl = 'https://wa.me/18296932458';

  getStatusLabel(status: string | null | undefined): string {
    switch (status) {
      case 'active':   return 'Activa';
      case 'trialing': return 'Período de prueba';
      case 'expired':  return 'Expirada';
      default:         return 'Activa';
    }
  }
}
