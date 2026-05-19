import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { GatewayService } from './gateway.service';
import { AuthService } from './auth.service';

export interface TopProduct {
  product_name: string;
  units_sold: number;
  revenue: number;
}

export interface SalesReport {
  tenant_id: string;
  period_from: string;
  period_to: string;
  total_revenue: number;
  transaction_count: number;
  average_ticket: number;
  top_products: TopProduct[];
  generated_at: string;
}

export interface LowStockAlert {
  product_id: string;
  product_name: string;
  category_name: string | null;
  stock_quantity: number;
  sale_price: number;
}

export interface InventoryAlertsReport {
  tenant_id: string;
  low_stock: LowStockAlert[];
  generated_at: string;
}

export interface CustomerReport {
  tenant_id: string;
  total_customers: number;
  with_debt: number;
  total_debt: number;
  generated_at: string;
}

@Injectable({ providedIn: 'root' })
export class ReportsService {
  private gateway = inject(GatewayService);
  private auth = inject(AuthService);

  private tenantId(): string {
    return this.auth.currentUser()?.tenantId ?? '';
  }

  getSalesReport(from?: string, to?: string): Observable<SalesReport> {
    const params: any = { tenant_id: this.tenantId() };
    if (from) params['from'] = from;
    if (to) params['to'] = to;
    return this.gateway.get<SalesReport>('/api/v1/reports/sales', params);
  }

  getInventoryAlerts(): Observable<InventoryAlertsReport> {
    return this.gateway.get<InventoryAlertsReport>('/api/v1/reports/inventory-alerts', {
      tenant_id: this.tenantId(),
    });
  }

  getCustomerReport(): Observable<CustomerReport> {
    return this.gateway.get<CustomerReport>('/api/v1/reports/customers', {
      tenant_id: this.tenantId(),
    });
  }
}
