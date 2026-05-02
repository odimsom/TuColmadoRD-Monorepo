import { Component, inject, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { SaleService, SaleSummary } from '../../../core/services/sale.service';
import { InventoryService } from '../../../core/services/inventory.service';
import { CustomerService } from '../../../core/services/customer.service';
import { RdCurrencyPipe } from '../../../core/pipes';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, RdCurrencyPipe],
  templateUrl: './dashboard.html',
})
export class Dashboard implements OnInit, OnDestroy {
  private saleService = inject(SaleService);
  private inventoryService = inject(InventoryService);
  private customerService = inject(CustomerService);
  private subs = new Subscription();

  sales = [] as SaleSummary[];
  searchTerm = '';
  loading = true;

  get filteredSales(): SaleSummary[] {
    if (!this.searchTerm) return this.sales;
    const term = this.searchTerm.toLowerCase();
    return this.sales.filter(s => s.receiptNumber.toLowerCase().includes(term));
  }

  onSearch(event: Event): void {
    this.searchTerm = (event.target as HTMLInputElement).value;
  }

  stats = {
    totalSales: 0,
    totalRevenue: 0,
    activeCustomers: 0
  };

  debtStats = { customersWithDebt: 0, totalDebt: 0 };
  debtLoading = true;

  lowStock: { count: number; items: { productId: string; name: string; stockQuantity: number }[] } = { count: 0, items: [] };
  lowStockLoading = true;

  // Turno activo
  shift: {
    id?: string;
    startedAt?: string;
    elapsedDisplay?: string;
    hasActiveShift: boolean;
  } = { hasActiveShift: false };

  private shiftTimer?: ReturnType<typeof setInterval>;

  ngOnInit(): void {
    this.loadSales();
    this.loadShift();
    this.loadLowStock();
    this.loadDebtStats();
  }

  ngOnDestroy(): void {
    this.subs.unsubscribe();
    if (this.shiftTimer) clearInterval(this.shiftTimer);
  }

  loadSales(): void {
    this.saleService.getSales(1, 10).subscribe({
      next: (res) => {
        this.sales = res.items;
        this.stats.totalSales = res.totalCount;
        this.stats.totalRevenue = res.totalRevenue;
        this.loading = false;
      },
      error: (err) => {
        this.loading = false;
      }
    });
  }

  loadDebtStats(): void {
    this.debtLoading = true;
    this.customerService.getCustomers().subscribe({
      next: (customers) => {
        const debtors = customers.filter(c => c.balance < 0);
        this.debtStats = {
          customersWithDebt: debtors.length,
          totalDebt: debtors.reduce((sum, c) => sum + Math.abs(c.balance), 0),
        };
        this.debtLoading = false;
      },
      error: () => { this.debtLoading = false; },
    });
  }

  loadLowStock(): void {
    this.lowStockLoading = true;
    this.inventoryService.getLowStock(5).subscribe({
      next: data => {
        this.lowStock = data;
        this.lowStockLoading = false;
      },
      error: () => this.lowStockLoading = false
    });
  }

  loadShift(): void {
    this.saleService.getCurrentShift().subscribe({
      next: (shiftData) => {
        if (shiftData) {
          this.shift = {
            id: shiftData.shiftId,
            startedAt: shiftData.openedAt,
            hasActiveShift: true,
            elapsedDisplay: this.calcElapsed(shiftData.openedAt)
          };
          // Actualizar el contador cada segundo
          this.shiftTimer = setInterval(() => {
            this.shift.elapsedDisplay = this.calcElapsed(this.shift.startedAt!);
          }, 1000);
        } else {
          this.shift = { hasActiveShift: false };
        }
      },
      error: () => {
        this.shift = { hasActiveShift: false };
      }
    });
  }

  private calcElapsed(startedAt: string): string {
    const start = new Date(startedAt).getTime();
    const now = Date.now();
    const diff = Math.max(0, Math.floor((now - start) / 1000));
    const h = Math.floor(diff / 3600).toString().padStart(2, '0');
    const m = Math.floor((diff % 3600) / 60).toString().padStart(2, '0');
    const s = (diff % 60).toString().padStart(2, '0');
    return `${h}:${m}:${s}`;
  }

  getStatusClass(status: number): string {
    switch (status) {
      case 1: return 'badge-success';
      case 2: return 'badge-error';
      default: return 'badge-ghost';
    }
  }

  getStatusText(status: number): string {
    switch (status) {
      case 1: return 'Completada';
      case 2: return 'Anulada';
      default: return 'Pendiente';
    }
  }
}