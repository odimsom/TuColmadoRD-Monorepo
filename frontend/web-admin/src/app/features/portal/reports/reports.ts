import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ReportsService, SalesReport, InventoryAlertsReport, CustomerReport } from '../../../core/services/reports.service';
import { RdCurrencyPipe } from '../../../core/pipes';

type Tab = 'sales' | 'inventory' | 'customers';

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [CommonModule, FormsModule, RdCurrencyPipe],
  templateUrl: './reports.html',
})
export class Reports implements OnInit {
  private reportsService = inject(ReportsService);

  activeTab = signal<Tab>('sales');

  // Date range for sales report
  today = new Date().toISOString().split('T')[0];
  monthStart = `${new Date().getFullYear()}-${String(new Date().getMonth() + 1).padStart(2, '0')}-01`;
  dateFrom = this.monthStart;
  dateTo = this.today;

  // Sales
  salesReport = signal<SalesReport | null>(null);
  salesLoading = signal(false);
  salesError = signal<string | null>(null);

  // Inventory
  inventoryReport = signal<InventoryAlertsReport | null>(null);
  inventoryLoading = signal(false);
  inventoryError = signal<string | null>(null);

  // Customers
  customerReport = signal<CustomerReport | null>(null);
  customerLoading = signal(false);
  customerError = signal<string | null>(null);

  debtPercent = computed(() => {
    const r = this.customerReport();
    if (!r || r.total_customers === 0) return 0;
    return Math.round((r.with_debt / r.total_customers) * 100);
  });

  ngOnInit(): void {
    this.loadSalesReport();
    this.loadInventoryReport();
    this.loadCustomerReport();
  }

  setTab(tab: Tab): void {
    this.activeTab.set(tab);
  }

  loadSalesReport(): void {
    this.salesLoading.set(true);
    this.salesError.set(null);
    this.reportsService.getSalesReport(this.dateFrom, this.dateTo).subscribe({
      next: r => { this.salesReport.set(r); this.salesLoading.set(false); },
      error: () => { this.salesError.set('No se pudo cargar el reporte de ventas.'); this.salesLoading.set(false); },
    });
  }

  loadInventoryReport(): void {
    this.inventoryLoading.set(true);
    this.inventoryError.set(null);
    this.reportsService.getInventoryAlerts().subscribe({
      next: r => { this.inventoryReport.set(r); this.inventoryLoading.set(false); },
      error: () => { this.inventoryError.set('No se pudo cargar el reporte de inventario.'); this.inventoryLoading.set(false); },
    });
  }

  loadCustomerReport(): void {
    this.customerLoading.set(true);
    this.customerError.set(null);
    this.reportsService.getCustomerReport().subscribe({
      next: r => { this.customerReport.set(r); this.customerLoading.set(false); },
      error: () => { this.customerError.set('No se pudo cargar el reporte de clientes.'); this.customerLoading.set(false); },
    });
  }

  onDateChange(): void {
    this.loadSalesReport();
  }

  stockClass(qty: number): string {
    if (qty === 0) return 'text-red-400';
    if (qty <= 2) return 'text-red-300';
    return 'text-amber-400';
  }

  stockBadge(qty: number): string {
    if (qty === 0) return 'Sin stock';
    if (qty <= 2) return 'Crítico';
    return 'Bajo';
  }

  stockBadgeClass(qty: number): string {
    if (qty === 0) return 'bg-red-500/20 text-red-400 ring-red-500/30';
    if (qty <= 2) return 'bg-red-500/10 text-red-300 ring-red-300/20';
    return 'bg-amber-500/10 text-amber-400 ring-amber-500/20';
  }
}
