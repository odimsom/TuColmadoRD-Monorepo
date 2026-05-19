import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MonetaryFundService, MonetaryFundDto, FundTransactionDto, FundBalanceResponse } from '../../../core/services/monetary-fund.service';
import { RdCurrencyPipe } from '../../../core/pipes';
import {
  FUND_TRANSACTION_TYPE, FUND_TRANSACTION_TYPE_LABELS,
  EXPENSE_CATEGORY, EXPENSE_CATEGORY_LABELS,
  DEFAULT_PAGE_SIZE,
} from '../../../core/constants';

@Component({
  selector: 'app-monetary-fund',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RdCurrencyPipe],
  templateUrl: './monetary-fund.html',
})
export class MonetaryFund implements OnInit {
  private fundService = inject(MonetaryFundService);
  private fb = inject(FormBuilder);

  funds = signal<MonetaryFundDto[]>([]);
  selectedFund = signal<FundBalanceResponse | null>(null);
  transactions = signal<FundTransactionDto[]>([]);
  loading = signal(true);
  saving = signal(false);
  errorMsg = signal<string | null>(null);
  successMsg = signal<string | null>(null);

  showCreateModal = signal(false);
  showDepositModal = signal(false);
  showExpenseModal = signal(false);

  page = signal(1);
  totalCount = signal(0);
  totalPages = signal(0);

  createForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(80)]],
    initialDeposit: [0, [Validators.required, Validators.min(0)]],
  });

  depositForm = this.fb.nonNullable.group({
    amount: [0, [Validators.required, Validators.min(0.01)]],
    description: ['', [Validators.required, Validators.minLength(3)]],
  });

  expenseForm = this.fb.nonNullable.group({
    amount: [0, [Validators.required, Validators.min(0.01)]],
    category: [EXPENSE_CATEGORY.OPERATIONAL, Validators.required],
    description: ['', [Validators.required, Validators.minLength(3)]],
    justificationNote: [''],
  });

  readonly expenseCategories = Object.entries(EXPENSE_CATEGORY).map(([key, value]) => ({
    key, value, label: EXPENSE_CATEGORY_LABELS[value as number]
  }));

  totalBalance = computed(() => this.funds().reduce((s, f) => s + f.currentBalance, 0));
  activeFundCount = computed(() => this.funds().length);

  ngOnInit(): void {
    this.loadFunds();
  }

  loadFunds(): void {
    this.loading.set(true);
    this.fundService.getFunds().subscribe({
      next: funds => { this.funds.set(funds); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  selectFund(fund: MonetaryFundDto): void {
    this.selectedFund.set(null);
    this.transactions.set([]);
    this.fundService.getFund(fund.id).subscribe({
      next: data => {
        this.selectedFund.set(data);
        this.transactions.set(data.recentTransactions ?? []);
      },
      error: () => {},
    });
  }

  loadTransactions(fundId: string): void {
    this.fundService.getTransactions(fundId, this.page(), DEFAULT_PAGE_SIZE).subscribe({
      next: res => {
        this.transactions.set(res.items);
        this.totalCount.set(res.totalCount);
        this.totalPages.set(res.totalPages);
      },
      error: () => {},
    });
  }

  openCreate(): void {
    this.errorMsg.set(null);
    this.createForm.reset({ name: '', initialDeposit: 0 });
    this.showCreateModal.set(true);
  }

  openDeposit(): void {
    this.errorMsg.set(null);
    this.depositForm.reset({ amount: 0, description: '' });
    this.showDepositModal.set(true);
  }

  openExpense(): void {
    this.errorMsg.set(null);
    this.expenseForm.reset({ amount: 0, category: EXPENSE_CATEGORY.OPERATIONAL, description: '', justificationNote: '' });
    this.showExpenseModal.set(true);
  }

  closeModals(): void {
    this.showCreateModal.set(false);
    this.showDepositModal.set(false);
    this.showExpenseModal.set(false);
  }

  submitCreate(): void {
    if (this.createForm.invalid || this.saving()) { this.createForm.markAllAsTouched(); return; }
    this.saving.set(true);
    this.errorMsg.set(null);
    const v = this.createForm.getRawValue();
    this.fundService.createFund({ name: v.name, initialDeposit: v.initialDeposit }).subscribe({
      next: () => {
        this.saving.set(false);
        this.successMsg.set(`Fondo "${v.name}" creado.`);
        this.closeModals();
        this.loadFunds();
      },
      error: (err) => {
        this.saving.set(false);
        this.errorMsg.set(err?.error?.detail ?? err?.error?.message ?? 'Error creando fondo.');
      },
    });
  }

  submitDeposit(): void {
    const fund = this.selectedFund();
    if (!fund || this.depositForm.invalid || this.saving()) { this.depositForm.markAllAsTouched(); return; }
    this.saving.set(true);
    this.errorMsg.set(null);
    const v = this.depositForm.getRawValue();
    this.fundService.recordDeposit(fund.fund.id, { amount: v.amount, description: v.description }).subscribe({
      next: () => {
        this.saving.set(false);
        this.successMsg.set(`Depósito de ${v.amount | 0} registrado.`);
        this.closeModals();
        this.selectFund(fund.fund);
      },
      error: (err) => {
        this.saving.set(false);
        this.errorMsg.set(err?.error?.detail ?? err?.error?.message ?? 'Error registrando depósito.');
      },
    });
  }

  submitExpense(): void {
    const fund = this.selectedFund();
    if (!fund || this.expenseForm.invalid || this.saving()) { this.expenseForm.markAllAsTouched(); return; }
    this.saving.set(true);
    this.errorMsg.set(null);
    const v = this.expenseForm.getRawValue();
    const currentBalance = fund.fund.currentBalance;

    if (v.amount > currentBalance && !v.justificationNote.trim()) {
      this.errorMsg.set('Debes incluir una justificación si el gasto excede el balance.');
      this.saving.set(false);
      return;
    }

    this.fundService.recordExpense(fund.fund.id, {
      amount: v.amount,
      category: v.category,
      description: v.description,
      justificationNote: v.justificationNote || null,
    }).subscribe({
      next: () => {
        this.saving.set(false);
        this.successMsg.set('Gasto registrado.');
        this.closeModals();
        this.selectFund(fund.fund);
      },
      error: (err) => {
        this.saving.set(false);
        this.errorMsg.set(err?.error?.detail ?? err?.error?.message ?? 'Error registrando gasto.');
      },
    });
  }

  transactionTypeLabel(type: number): string {
    return FUND_TRANSACTION_TYPE_LABELS[type] ?? String(type);
  }

  transactionTypeColor(type: number): string {
    return type === FUND_TRANSACTION_TYPE.DEPOSIT
      ? 'text-green-400'
      : 'text-red-400';
  }

  categoryLabel(cat: number | null): string {
    if (cat === null) return '—';
    return EXPENSE_CATEGORY_LABELS[cat] ?? String(cat);
  }

  fmtDate(d: string): string {
    return new Intl.DateTimeFormat('es-DO', {
      day: '2-digit', month: 'short', year: 'numeric', hour: '2-digit', minute: '2-digit'
    }).format(new Date(d));
  }

  nextPage(): void {
    if (this.page() < this.totalPages()) {
      this.page.update(p => p + 1);
      const fund = this.selectedFund();
      if (fund) this.loadTransactions(fund.fund.id);
    }
  }

  prevPage(): void {
    if (this.page() > 1) {
      this.page.update(p => p - 1);
      const fund = this.selectedFund();
      if (fund) this.loadTransactions(fund.fund.id);
    }
  }
}
