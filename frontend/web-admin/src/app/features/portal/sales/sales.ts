import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder } from '@angular/forms';
import { SaleService, SaleSummary, ShiftDto } from '../../../core/services/sale.service';

type ActiveTab = 'shifts' | 'transactions';

@Component({
  selector: 'app-sales',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './sales.html',
})
export class Sales implements OnInit {
  private saleService = inject(SaleService);
  private fb = inject(FormBuilder);

  // Tabs
  activeTab = signal<ActiveTab>('shifts');

  // Shifts
  shifts = signal<ShiftDto[]>([]);
  loadingShifts = signal(true);
  shiftPage = signal(1);
  shiftTotalPages = signal(0);
  shiftStatus = signal('all');

  // Transactions (ventas)
  sales = signal<SaleSummary[]>([]);
  loadingTrans = signal(true);
  transPage = signal(1);
  transTotalPages = signal(0);
  transTotalCount = signal(0);

  // Open shift modal
  showOpenShift = signal(false);
  savingShift = signal(false);
  openShiftError = signal<string | null>(null);

  openShiftForm = this.fb.nonNullable.group({
    openingCashAmount: [0],
    cashierName: [''],
  });

  ngOnInit(): void {
    this.loadShifts();
    this.loadSales();
  }

  setTab(tab: ActiveTab): void {
    this.activeTab.set(tab);
  }

  // --- Shifts ---
  loadShifts(): void {
    this.loadingShifts.set(true);
    this.saleService.getShiftsPaged(this.shiftPage(), 15, this.shiftStatus()).subscribe({
      next: (res) => {
        this.shifts.set(res.items ?? []);
        this.shiftTotalPages.set(res.totalPages ?? 0);
        this.loadingShifts.set(false);
      },
      error: () => this.loadingShifts.set(false)
    });
  }

  filterShifts(status: string): void {
    this.shiftStatus.set(status);
    this.shiftPage.set(1);
    this.loadShifts();
  }

  prevShiftPage(): void {
    if (this.shiftPage() > 1) { this.shiftPage.update(p => p - 1); this.loadShifts(); }
  }
  nextShiftPage(): void {
    if (this.shiftPage() < this.shiftTotalPages()) { this.shiftPage.update(p => p + 1); this.loadShifts(); }
  }

  openShiftModal(): void {
    this.openShiftForm.reset({ openingCashAmount: 0, cashierName: '' });
    this.openShiftError.set(null);
    this.showOpenShift.set(true);
  }

  submitOpenShift(): void {
    const v = this.openShiftForm.getRawValue();
    this.savingShift.set(true);
    this.saleService.openShift({ openingCashAmount: v.openingCashAmount, cashierName: v.cashierName }).subscribe({
      next: () => {
        this.savingShift.set(false);
        this.showOpenShift.set(false);
        this.loadShifts();
      },
      error: (err) => {
        this.savingShift.set(false);
        this.openShiftError.set(err?.error?.message || 'Error abriendo turno.');
      }
    });
  }

  // --- Transactions ---
  loadSales(): void {
    this.loadingTrans.set(true);
    this.saleService.getSales(this.transPage(), 15).subscribe({
      next: (res) => {
        this.sales.set(res.items);
        this.transTotalPages.set(res.totalPages);
        this.transTotalCount.set(res.totalCount);
        this.loadingTrans.set(false);
      },
      error: () => this.loadingTrans.set(false)
    });
  }

  prevTransPage(): void {
    if (this.transPage() > 1) { this.transPage.update(p => p - 1); this.loadSales(); }
  }
  nextTransPage(): void {
    if (this.transPage() < this.transTotalPages()) { this.transPage.update(p => p + 1); this.loadSales(); }
  }

  voidSale(id: string): void {
    const reason = prompt('Ingrese el motivo de la anulación (Requerido por DGII):');
    if (reason && reason.trim()) {
      this.saleService.voidSale(id, reason.trim()).subscribe({ next: () => this.loadSales() });
    }
  }

  getStatusClass(status: number): string {
    switch (status) {
      case 1: return 'text-green-400 bg-green-500/10 ring-green-500/20';
      case 2: return 'text-red-400 bg-red-500/10 ring-red-500/20';
      default: return 'text-slate-400 bg-slate-500/10 ring-slate-500/20';
    }
  }

  getStatusText(status: number): string {
    switch (status) {
      case 1: return 'Completada';
      case 2: return 'Anulada';
      default: return 'Pendiente';
    }
  }

  getShiftStatusClass(status: string): string {
    return status?.toLowerCase() === 'open'
      ? 'text-green-400 bg-green-500/10 ring-green-500/20'
      : 'text-slate-400 bg-slate-500/10 ring-slate-500/20';
  }
}
