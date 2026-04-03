import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SaleService, SaleSummary } from '../../../core/services/sale.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.html',
})
export class Dashboard implements OnInit {
  private saleService = inject(SaleService);
  
  sales = [] as SaleSummary[];
  loading = true;
  stats = {
    totalSales: 0,
    totalRevenue: 0,
    activeCustomers: 0
  };

  ngOnInit(): void {
    this.loadSales();
  }

  loadSales(): void {
    this.saleService.getSales(1, 10).subscribe({
      next: (res) => {
        this.sales = res.items;
        this.stats.totalSales = res.totalCount;
        this.stats.totalRevenue = res.items.reduce((acc, s) => acc + s.totalAmount, 0);
        this.loading = false;
      },
      error: (err) => {
        console.error('Error loading sales:', err);
        this.loading = false;
      }
    });
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
