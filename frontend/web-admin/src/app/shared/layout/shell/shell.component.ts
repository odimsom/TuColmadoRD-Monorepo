import { Component, ChangeDetectionStrategy, signal, inject } from '@angular/core';
import { RouterOutlet, ActivatedRoute, Router, NavigationEnd } from '@angular/router';
import { filter, map } from 'rxjs';
import { toSignal } from '@angular/core/rxjs-interop';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { TopbarComponent } from '../topbar/topbar.component';

const ROUTE_TITLES: Record<string, string> = {
  dashboard: 'Dashboard',
  inventario: 'Inventario',
  ventas: 'Ventas',
  compras: 'Compras',
  cajas: 'Cajas',
  clientes: 'Clientes',
  empleados: 'Empleados',
  deliveries: 'Deliveries',
  reportes: 'Reportes',
  configuracion: 'Configuración',
};

@Component({
  selector: 'app-shell',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterOutlet, SidebarComponent, TopbarComponent],
  template: `
    <div class="flex h-screen overflow-hidden bg-base-200">

      <!-- Desktop sidebar -->
      <div class="hidden lg:flex shrink-0 h-full shadow-lg">
        <app-sidebar />
      </div>

      <!-- Mobile sidebar overlay -->
      @if (_mobileOpen()) {
        <div
          class="fixed inset-0 z-30 bg-black/60 lg:hidden"
          (click)="_mobileOpen.set(false)"
          role="presentation"
        ></div>
        <div class="fixed inset-y-0 left-0 z-40 lg:hidden">
          <app-sidebar (closeMobile)="_mobileOpen.set(false)" />
        </div>
      }

      <!-- Main area -->
      <div class="flex flex-col flex-1 min-w-0 overflow-hidden">
        <app-topbar
          [title]="_pageTitle()"
          (menuToggle)="_mobileOpen.set(!_mobileOpen())"
        />
        <main class="flex-1 overflow-y-auto p-4 lg:p-6">
          <router-outlet />
        </main>
      </div>
    </div>
  `,
})
export class ShellComponent {
  private router = inject(Router);
  _mobileOpen = signal(false);

  _pageTitle = toSignal(
    this.router.events.pipe(
      filter(e => e instanceof NavigationEnd),
      map(e => {
        const url = (e as NavigationEnd).urlAfterRedirects;
        const segment = url.split('/').filter(Boolean).at(-1) ?? '';
        return ROUTE_TITLES[segment] ?? 'Portal';
      })
    ),
    { initialValue: 'Portal' }
  );
}
