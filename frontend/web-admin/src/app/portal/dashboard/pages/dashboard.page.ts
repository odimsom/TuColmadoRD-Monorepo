import { Component, ChangeDetectionStrategy, inject, computed, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth.service';
import { CardComponent } from '../../../shared/ui/card/card.component';

interface QuickLink {
  label: string;
  route: string;
  icon: string;
  description: string;
}

const QUICK_LINKS: QuickLink[] = [
  { label: 'Inventario', route: '/portal/inventario', icon: 'lucide:package', description: 'Gestiona productos' },
  { label: 'Ventas', route: '/portal/ventas', icon: 'lucide:shopping-cart', description: 'Historial de ventas' },
  { label: 'Cajas', route: '/portal/cajas', icon: 'lucide:wallet', description: 'Fondo y gastos' },
  { label: 'Clientes', route: '/portal/clientes', icon: 'lucide:users', description: 'Fiados y clientes' },
  { label: 'Compras', route: '/portal/compras', icon: 'lucide:truck', description: 'Entradas de stock' },
  { label: 'Reportes', route: '/portal/reportes', icon: 'lucide:bar-chart-2', description: 'Analítica' },
];

@Component({
  selector: 'app-dashboard-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  imports: [RouterLink, CardComponent],
  template: `
    <div class="space-y-6">
      <!-- Welcome -->
      <div class="flex items-center gap-3">
        <div class="w-10 h-10 bg-primary/10 flex items-center justify-center rounded-lg shrink-0">
          <iconify-icon icon="lucide:sparkles" class="text-primary text-xl"></iconify-icon>
        </div>
        <div>
          <h2 class="text-2xl font-black text-base-content tracking-tight leading-none">
            Buenas, {{ _name() }}
          </h2>
          <p class="text-base-content/50 text-sm mt-1">
            ¿Qué vamos a revisar hoy?
          </p>
        </div>
      </div>

      <!-- Quick access -->
      <div class="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-6 gap-4">
        @for (link of quickLinks; track link.route) {
          <a
            [routerLink]="link.route"
            class="flex flex-col items-center gap-2 p-4 bg-base-100 border border-base-300
                   hover:border-primary/40 hover:bg-primary/5 transition-colors group rounded-lg"
          >
            <div class="w-10 h-10 bg-primary/10 flex items-center justify-center">
              <iconify-icon [attr.icon]="link.icon" class="text-primary text-xl"></iconify-icon>
            </div>
            <div class="text-center">
              <p class="text-xs font-bold text-base-content">{{ link.label }}</p>
              <p class="text-[10px] text-base-content/40">{{ link.description }}</p>
            </div>
          </a>
        }
      </div>

      <!-- Placeholder stats -->
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        @for (stat of stats; track stat.label) {
          <app-card [compact]="true" [bordered]="true">
            <div class="flex items-start justify-between">
              <div>
                <p class="text-base-content/50 text-xs uppercase tracking-widest font-bold">{{ stat.label }}</p>
                <p class="text-2xl font-black text-base-content mt-1">{{ stat.value }}</p>
              </div>
              <iconify-icon [attr.icon]="stat.icon" class="text-xl text-base-content/20 mt-0.5"></iconify-icon>
            </div>
          </app-card>
        }
      </div>
    </div>
  `,
})
export class DashboardPage {
  private auth = inject(AuthService);
  quickLinks = QUICK_LINKS;

  _name = computed(() => {
    const u = this.auth.currentUser();
    return u?.firstName ?? u?.email?.split('@')[0] ?? 'usuario';
  });

  stats = [
    { label: 'Ventas hoy',        value: '—', icon: 'lucide:shopping-cart' },
    { label: 'Productos',         value: '—', icon: 'lucide:package' },
    { label: 'Clientes activos',  value: '—', icon: 'lucide:users' },
    { label: 'Fiados pendientes', value: '—', icon: 'lucide:clipboard-list' },
  ];
}
