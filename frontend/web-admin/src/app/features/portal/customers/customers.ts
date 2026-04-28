import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import {
  CustomerService,
  CustomerSummary,
  CustomerStatementEntry,
} from '../../../core/services/customer.service';
import { RdCurrencyPipe, RdPhonePipe } from '../../../core/pipes';

type ModalMode = 'create' | 'payment' | 'statement' | null;

@Component({
  selector: 'app-customers',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RdCurrencyPipe, RdPhonePipe],
  templateUrl: './customers.html',
})
export class Customers implements OnInit {
  private customerService = inject(CustomerService);
  private fb = inject(FormBuilder);

  customers = signal<CustomerSummary[]>([]);
  loading = signal(true);
  saving = signal(false);
  errorMsg = signal<string | null>(null);
  searchTerm = signal('');

  modalMode = signal<ModalMode>(null);
  selected = signal<CustomerSummary | null>(null);
  statement = signal<CustomerStatementEntry[]>([]);
  loadingStatement = signal(false);

  filtered = computed(() => {
    const term = this.searchTerm().toLowerCase();
    if (!term) return this.customers();
    return this.customers().filter(
      c => c.fullName.toLowerCase().includes(term) || c.phone?.includes(term)
    );
  });

  totalDebt = computed(() =>
    this.customers().reduce((sum, c) => sum + (c.balance < 0 ? -c.balance : 0), 0)
  );
  customersWithDebt = computed(() => this.customers().filter(c => c.balance < 0).length);

  createForm = this.fb.nonNullable.group({
    fullName:    ['', [Validators.required, Validators.minLength(2), Validators.maxLength(150)]],
    documentId:  ['', [Validators.required, Validators.pattern(/^\d{3}-?\d{7}-?\d$|^\d{11}$/)]],
    phone:       [''],
    creditLimit: [0, [Validators.min(0)]],
  });

  paymentForm = this.fb.nonNullable.group({
    amount:  [0, [Validators.required, Validators.min(0.01)]],
    concept: ['Abono', [Validators.required]],
  });

  ngOnInit(): void {
    this.loadCustomers();
  }

  loadCustomers(): void {
    this.loading.set(true);
    this.customerService.getCustomers().subscribe({
      next: (list) => { this.customers.set(list); this.loading.set(false); },
      error: ()    => { this.loading.set(false); },
    });
  }

  onSearch(e: Event): void {
    this.searchTerm.set((e.target as HTMLInputElement).value);
  }

  openCreate(): void {
    this.createForm.reset({ creditLimit: 0 });
    this.errorMsg.set(null);
    this.modalMode.set('create');
  }

  openPayment(c: CustomerSummary): void {
    this.selected.set(c);
    this.paymentForm.reset({ amount: 0, concept: 'Abono' });
    this.errorMsg.set(null);
    this.modalMode.set('payment');
  }

  openStatement(c: CustomerSummary): void {
    this.selected.set(c);
    this.statement.set([]);
    this.loadingStatement.set(true);
    this.modalMode.set('statement');
    this.customerService.getStatement(c.customerId).subscribe({
      next: (entries) => { this.statement.set(entries); this.loadingStatement.set(false); },
      error: ()       => { this.loadingStatement.set(false); },
    });
  }

  closeModal(): void {
    this.modalMode.set(null);
    this.selected.set(null);
    this.errorMsg.set(null);
  }

  submitCreate(): void {
    if (this.createForm.invalid) { this.createForm.markAllAsTouched(); return; }
    this.saving.set(true);
    const v = this.createForm.getRawValue();
    this.customerService.createCustomer({
      fullName:    v.fullName.trim(),
      documentId:  v.documentId.trim(),
      phone:       v.phone?.trim() || null,
      creditLimit: v.creditLimit > 0 ? v.creditLimit : null,
    }).subscribe({
      next: () => { this.saving.set(false); this.closeModal(); this.loadCustomers(); },
      error: (err) => { this.saving.set(false); this.errorMsg.set(err?.error?.message || 'Error creando cliente.'); },
    });
  }

  submitPayment(): void {
    if (this.paymentForm.invalid || !this.selected()) return;
    this.saving.set(true);
    const v = this.paymentForm.getRawValue();
    this.customerService.registerPayment(this.selected()!.customerId, {
      amount: v.amount,
      paymentMethodId: 1,
      concept: v.concept.trim(),
    }).subscribe({
      next: () => { this.saving.set(false); this.closeModal(); this.loadCustomers(); },
      error: (err) => { this.saving.set(false); this.errorMsg.set(err?.error?.message || 'Error registrando pago.'); },
    });
  }

  getBalanceClass(balance: number): string {
    if (balance < 0) return 'text-red-400';
    if (balance > 0) return 'text-green-400';
    return 'text-slate-400';
  }

  getTypeLabel(type: string): string {
    switch (type.toLowerCase()) {
      case 'debt':    return 'Fiado';
      case 'payment': return 'Abono';
      default:        return type;
    }
  }

  getTypeClass(type: string): string {
    switch (type.toLowerCase()) {
      case 'debt':    return 'text-red-400 bg-red-500/10 ring-red-500/20';
      case 'payment': return 'text-green-400 bg-green-500/10 ring-green-500/20';
      default:        return 'text-slate-400 bg-slate-500/10 ring-slate-500/20';
    }
  }
}
