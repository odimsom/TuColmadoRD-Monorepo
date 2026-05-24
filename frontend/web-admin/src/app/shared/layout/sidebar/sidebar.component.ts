import { Component, ChangeDetectionStrategy, inject, output, CUSTOM_ELEMENTS_SCHEMA, computed } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../../core/auth.service';
import { TcLogoComponent } from '../../ui/logo/tc-logo.component';

interface NavItem {
  label: string;
  route: string;
  icon: string;
}

const NAV_ITEMS: NavItem[] = [
  { label: 'Dashboard', route: '/portal/dashboard', icon: 'lucide:layout-dashboard' },
  { label: 'Inventario', route: '/portal/inventario', icon: 'lucide:package' },
  { label: 'Ventas', route: '/portal/ventas', icon: 'lucide:shopping-cart' },
  { label: 'Compras', route: '/portal/compras', icon: 'lucide:truck' },
  { label: 'Cajas', route: '/portal/cajas', icon: 'lucide:wallet' },
  { label: 'Clientes', route: '/portal/clientes', icon: 'lucide:users' },
  { label: 'Empleados', route: '/portal/empleados', icon: 'lucide:user-cog' },
  { label: 'Deliveries', route: '/portal/deliveries', icon: 'lucide:bike' },
  { label: 'Reportes', route: '/portal/reportes', icon: 'lucide:bar-chart-2' },
  { label: 'Configuración', route: '/portal/configuracion', icon: 'lucide:settings' },
];

@Component({
  selector: 'app-sidebar',
  changeDetection: ChangeDetectionStrategy.OnPush,
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  imports: [RouterLink, RouterLinkActive, TcLogoComponent],
  template: `
    <aside class="flex flex-col h-full w-64 bg-neutral text-neutral-content shadow-xl">

      <!-- Brand -->
      <div class="px-5 py-4 border-b border-white/10 shrink-0">
        <app-tc-logo size="sm" [onDark]="true" />
        <p class="text-[9px] font-bold text-white/40 uppercase tracking-[0.20em] mt-1.5 pl-0.5">
          Portal de gestión
        </p>
      </div>

      <!-- Nav -->
      <nav class="flex-1 overflow-y-auto py-3 px-1.5" aria-label="Navegación principal">
        @for (item of navItems; track item.route) {
          <a
            [routerLink]="item.route"
            routerLinkActive="!bg-primary/20 !text-primary !border-primary"
            [routerLinkActiveOptions]="{ exact: false }"
            class="flex items-center gap-3 px-3.5 py-2.5 text-[13px] font-medium text-white/70
                   hover:bg-white/5 hover:text-white transition-colors
                   border-l-2 border-transparent rounded-r-lg"
          >
            <iconify-icon [attr.icon]="item.icon" class="text-base shrink-0"></iconify-icon>
            {{ item.label }}
          </a>
        }
      </nav>

      <!-- User section -->
      <div class="p-4 border-t border-white/10 shrink-0">
        <div class="flex items-center gap-3">
          <div class="w-9 h-9 bg-primary text-white rounded-full flex items-center justify-center font-black text-xs shrink-0">
            {{ _initials() }}
          </div>
          <div class="flex-1 min-w-0">
            <p class="text-[13px] font-medium text-white truncate leading-tight">{{ _auth.currentUser()?.email }}</p>
            <p class="text-[11px] text-white/40 capitalize mt-0.5">{{ _auth.currentUser()?.role }}</p>
          </div>
          <button
            (click)="_auth.logout()"
            class="p-1.5 rounded-md text-white/60 hover:text-white hover:bg-white/5 transition-colors"
            aria-label="Cerrar sesión"
          >
            <iconify-icon icon="lucide:log-out" class="text-base"></iconify-icon>
          </button>
        </div>
      </div>
    </aside>
  `,
})
export class SidebarComponent {
  _auth = inject(AuthService);
  navItems = NAV_ITEMS;
  closeMobile = output<void>();

  _initials = computed(() => {
    const u = this._auth.currentUser();
    if (!u) return '?';
    const first = u.firstName?.[0] ?? u.email[0];
    const last = u.lastName?.[0] ?? '';
    return (first + last).toUpperCase();
  });
}
