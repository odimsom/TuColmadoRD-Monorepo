import { Component, ChangeDetectionStrategy, inject, computed, signal, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { RouterLink } from '@angular/router';
import { HttpClient, HttpParams } from '@angular/common/http';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AuthService } from '../../../core/auth.service';
import { CardComponent } from '../../../shared/ui/card/card.component';
import { SpinnerComponent } from '../../../shared/ui/spinner/spinner.component';
import { environment } from '../../../../environments/environment';

interface QuickLink {
  label: string;
  route: string;
  icon: string;
  description: string;
}

interface SalesReport {
  total_revenue: number;
  transaction_count: number;
}

interface CustomersReport {
  total_customers: number;
  with_debt: number;
  total_debt: number;
}

interface ProductsPage {
  totalCount: number;
}

const QUICK_LINKS: QuickLink[] = [
  { label: 'Inventario', route: '/portal/inventario', icon: 'lucide:package', description: 'Gestiona productos' },
  { label: 'Ventas', route: '/portal/ventas', icon: 'lucide:shopping-cart', description: 'Historial de ventas' },
  { label: 'Cajas', route: '/portal/cajas', icon: 'lucide:wallet', description: 'Fondo y gastos' },
  { label: 'Clientes', route: '/portal/clientes', icon: 'lucide:users', description: 'Fiados y clientes' },
  { label: 'Compras', route: '/portal/compras', icon: 'lucide:truck', description: 'Entradas de stock' },
  { label: 'Reportes', route: '/portal/reportes', icon: 'lucide:bar-chart-2', description: 'Analítica' },
];

function formatCurrency(value: number): string {
  return 'RD$ ' + value.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}

@Component({
  selector: 'app-dashboard-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  imports: [RouterLink, CardComponent, SpinnerComponent],
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
            class="flex flex-col items-center gap-2 p-4 tc-card
                   hover:border-primary/40 hover:bg-primary/5 transition-colors group"
          >
            <div class="w-10 h-10 bg-primary/10 flex items-center justify-center rounded-lg">
              <iconify-icon [attr.icon]="link.icon" class="text-primary text-xl"></iconify-icon>
            </div>
            <div class="text-center">
              <p class="text-xs font-bold text-base-content">{{ link.label }}</p>
              <p class="text-[10px] text-base-content/40 uppercase tracking-widest">{{ link.description }}</p>
            </div>
          </a>
        }
      </div>

      <!-- Stats -->
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        @if (loading()) {
          @for (stat of statsMeta; track stat.label) {
            <app-card [compact]="true">
              <div class="flex items-start justify-between">
                <div>
                  <p class="text-base-content/50 text-[10px] uppercase tracking-widest font-black">{{ stat.label }}</p>
                  <div class="mt-2">
                    <app-spinner size="sm" />
                  </div>
                </div>
                <div class="w-8 h-8 bg-base-200 rounded flex items-center justify-center shrink-0">
                  <iconify-icon [attr.icon]="stat.icon" class="text-xl text-base-content/30"></iconify-icon>
                </div>
              </div>
            </app-card>
          }
        } @else {
          @for (stat of stats(); track stat.label) {
            <app-card [compact]="true">
              <div class="flex items-start justify-between">
                <div>
                  <p class="text-base-content/50 text-[10px] uppercase tracking-widest font-black">{{ stat.label }}</p>
                  <p class="text-2xl font-black text-base-content mt-1 italic tracking-tight">{{ stat.value }}</p>
                </div>
                <div class="w-8 h-8 bg-base-200 rounded flex items-center justify-center shrink-0">
                  <iconify-icon [attr.icon]="stat.icon" class="text-xl text-base-content/30"></iconify-icon>
                </div>
              </div>
            </app-card>
          }
        }
      </div>
    </div>
  `,
})
export class DashboardPage {
  private auth = inject(AuthService);
  private http = inject(HttpClient);

  quickLinks = QUICK_LINKS;
  loading = signal(true);
  error = signal(false);

  readonly statsMeta = [
    { label: 'Ventas hoy',        icon: 'lucide:shopping-cart' },
    { label: 'Ingresos hoy',      icon: 'lucide:trending-up' },
    { label: 'Productos',         icon: 'lucide:package' },
    { label: 'Fiados pendientes', icon: 'lucide:clipboard-list' },
  ];

  stats = signal([
    { label: 'Ventas hoy',        value: '—', icon: 'lucide:shopping-cart' },
    { label: 'Ingresos hoy',      value: '—', icon: 'lucide:trending-up' },
    { label: 'Productos',         value: '—', icon: 'lucide:package' },
    { label: 'Fiados pendientes', value: '—', icon: 'lucide:clipboard-list' },
  ]);

  _name = computed(() => {
    const u = this.auth.currentUser();
    return u?.firstName ?? u?.email?.split('@')[0] ?? 'usuario';
  });

  constructor() { this.loadStats(); }

  private loadStats(): void {
    const tenantId = localStorage.getItem('tc_tenant') ?? '';
    const today = new Date().toISOString().slice(0, 10);
    const api = `${environment.gatewayUrl}/gateway/api/v1`;

    const salesParams = new HttpParams()
      .set('tenant_id', tenantId)
      .set('from', today)
      .set('to', today);

    const customersParams = new HttpParams()
      .set('tenant_id', tenantId);

    const productsParams = new HttpParams()
      .set('pageSize', '1');

    forkJoin({
      sales: this.http.get<SalesReport>(`${api}/reports/sales`, { params: salesParams }).pipe(catchError(() => of(null))),
      customers: this.http.get<CustomersReport>(`${api}/reports/customers`, { params: customersParams }).pipe(catchError(() => of(null))),
      products: this.http.get<ProductsPage>(`${api}/inventory/products`, { params: productsParams }).pipe(catchError(() => of(null))),
    }).subscribe((results) => {
      this.stats.set([
        { label: 'Ventas hoy',        value: results.sales?.transaction_count != null ? String(results.sales.transaction_count) : '—', icon: 'lucide:shopping-cart' },
        { label: 'Ingresos hoy',      value: results.sales?.total_revenue != null ? formatCurrency(results.sales.total_revenue) : '—', icon: 'lucide:trending-up' },
        { label: 'Productos',         value: results.products?.totalCount != null ? String(results.products.totalCount) : '—', icon: 'lucide:package' },
        { label: 'Fiados pendientes', value: results.customers?.with_debt != null ? String(results.customers.with_debt) : '—', icon: 'lucide:clipboard-list' },
      ]);
      this.loading.set(false);
    });
  }
}
