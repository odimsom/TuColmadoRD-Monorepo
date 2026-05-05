import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ExpenseService, ExpenseSummary } from '../../../core/services/expense.service';
import { RdCurrencyPipe } from '../../../core/pipes';

const CATEGORIES = [
  { value: 'Utilities',    label: 'Servicios (Agua/Luz/Tel)' },
  { value: 'Maintenance',  label: 'Mantenimiento' },
  { value: 'Supplies',     label: 'Insumos y suministros' },
  { value: 'Hielo',        label: 'Hielo' },
  { value: 'Personnel',    label: 'Personal' },
  { value: 'Other',        label: 'Otro' },
] as const;

@Component({
  selector: 'app-expenses',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RdCurrencyPipe],
  templateUrl: './expenses.html',
})
export class Expenses implements OnInit {
  private expenseService = inject(ExpenseService);
  private fb = inject(FormBuilder);

  expenses = signal<ExpenseSummary[]>([]);
  loading = signal(true);
  saving = signal(false);
  errorMsg = signal<string | null>(null);
  showModal = signal(false);

  readonly categories = CATEGORIES;

  form = this.fb.nonNullable.group({
    amount:      [0,  [Validators.required, Validators.min(0.01)]],
    category:    ['Other', Validators.required],
    description: ['',  [Validators.required, Validators.minLength(3), Validators.maxLength(200)]],
  });

  totalToday = computed(() => {
    const today = new Date().toDateString();
    return this.expenses()
      .filter(e => new Date(e.date).toDateString() === today)
      .reduce((s, e) => s + e.amount, 0);
  });

  countToday = computed(() =>
    this.expenses().filter(e => new Date(e.date).toDateString() === new Date().toDateString()).length
  );

  totalAll = computed(() => this.expenses().reduce((s, e) => s + e.amount, 0));

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.expenseService.getExpenses().subscribe({
      next: items => { this.expenses.set(items); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  openModal(): void {
    this.form.reset({ amount: 0, category: 'Other', description: '' });
    this.errorMsg.set(null);
    this.showModal.set(true);
  }

  submit(): void {
    if (this.form.invalid || this.saving()) { this.form.markAllAsTouched(); return; }
    this.saving.set(true);
    this.errorMsg.set(null);
    const v = this.form.getRawValue();
    this.expenseService.registerExpense({
      amount:      v.amount,
      category:    v.category,
      description: v.description.trim(),
    }).subscribe({
      next: () => { this.saving.set(false); this.showModal.set(false); this.load(); },
      error: (err) => {
        this.saving.set(false);
        this.errorMsg.set(err?.error?.detail ?? err?.error?.message ?? 'Error al registrar el gasto.');
      },
    });
  }

  categoryLabel(value: string): string {
    return CATEGORIES.find(c => c.value === value)?.label ?? value;
  }

  categoryColor(value: string): string {
    switch (value) {
      case 'Utilities':   return 'text-blue-400 bg-blue-500/10 ring-blue-500/20';
      case 'Maintenance': return 'text-orange-400 bg-orange-500/10 ring-orange-500/20';
      case 'Supplies':    return 'text-purple-400 bg-purple-500/10 ring-purple-500/20';
      case 'Hielo':       return 'text-cyan-400 bg-cyan-500/10 ring-cyan-500/20';
      case 'Personnel':   return 'text-emerald-400 bg-emerald-500/10 ring-emerald-500/20';
      default:            return 'text-slate-400 bg-slate-500/10 ring-slate-500/20';
    }
  }
}
