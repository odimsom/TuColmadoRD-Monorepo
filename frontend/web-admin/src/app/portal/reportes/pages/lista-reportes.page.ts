import {
  Component, ChangeDetectionStrategy, CUSTOM_ELEMENTS_SCHEMA,
} from '@angular/core';
import { RouterLink } from '@angular/router';
import { CardComponent } from '../../../shared/ui/card/card.component';

interface ReporteAcceso {
  label: string;
  description: string;
  icon: string;
  route: string;
  available: boolean;
}

const REPORTES: ReporteAcceso[] = [
  {
    label: 'Ventas del día',
    description: 'Resumen de ventas, pagos y caja del día',
    icon: 'lucide:shopping-cart',
    route: '/portal/ventas',
    available: true,
  },
  {
    label: 'Inventario',
    description: 'Stock actual, productos activos e inactivos',
    icon: 'lucide:package',
    route: '/portal/inventario',
    available: true,
  },
  {
    label: 'Clientes con fiado',
    description: 'Clientes con balance pendiente de pago',
    icon: 'lucide:clipboard-list',
    route: '/portal/clientes',
    available: true,
  },
  {
    label: 'Movimientos de caja',
    description: 'Depósitos y gastos por fondo monetario',
    icon: 'lucide:wallet',
    route: '/portal/cajas',
    available: true,
  },
  {
    label: 'Análisis de ventas',
    description: 'Tendencias, productos más vendidos, métricas avanzadas',
    icon: 'lucide:bar-chart-2',
    route: '',
    available: false,
  },
  {
    label: 'Rentabilidad',
    description: 'Margen de ganancia por producto y categoría',
    icon: 'lucide:trending-up',
    route: '',
    available: false,
  },
];

import { BadgeComponent } from '../../../shared/ui/badge/badge.component';

@Component({
  selector: 'app-lista-reportes',
  changeDetection: ChangeDetectionStrategy.OnPush,
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  imports: [RouterLink, BadgeComponent],
  template: `
    <div class="space-y-5">

      <!-- Header -->
      <div>
        <h2 class="text-2xl font-black text-base-content tracking-tight">Reportes</h2>
        <p class="text-sm text-base-content/50 mt-1">Accede a la información clave de tu negocio</p>
      </div>

      <!-- Grid -->
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
        @for (r of reportes; track r.label) {
          @if (r.available) {
            <a
              [routerLink]="r.route"
              class="tc-card block p-5 hover:border-primary/40 hover:bg-primary/5 transition-colors group"
            >
              <div class="w-10 h-10 bg-primary/10 flex items-center justify-center rounded-lg mb-3">
                <iconify-icon [attr.icon]="r.icon" class="text-primary text-xl"></iconify-icon>
              </div>
              <p class="font-bold text-base-content">{{ r.label }}</p>
              <p class="text-[11px] text-base-content/50 mt-1.5 uppercase font-bold tracking-wider leading-relaxed">
                {{ r.description }}
              </p>
            </a>
          } @else {
            <div class="tc-card p-5 opacity-50 cursor-not-allowed">
              <div class="w-10 h-10 bg-base-200 flex items-center justify-center rounded-lg mb-3">
                <iconify-icon [attr.icon]="r.icon" class="text-base-content/30 text-xl"></iconify-icon>
              </div>
              <div class="flex items-center gap-2">
                <p class="font-bold text-base-content">{{ r.label }}</p>
                <app-badge variant="ghost">Pronto</app-badge>
              </div>
              <p class="text-[11px] text-base-content/40 mt-1.5 uppercase font-bold tracking-wider leading-relaxed">
                {{ r.description }}
              </p>
            </div>
          }
        }
      </div>
    </div>
  `,
})
export class ListaReportesPage {
  readonly reportes = REPORTES;
}
