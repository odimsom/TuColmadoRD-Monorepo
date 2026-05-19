import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { InventoryService, ProductPresentationDto, StockEntryDto } from '../../../core/services/inventory.service';
import { MonetaryFundService, MonetaryFundDto } from '../../../core/services/monetary-fund.service';
import { RdCurrencyPipe } from '../../../core/pipes';
import { PRESENTATION_TYPE_LABELS, UNIT_OF_MEASURE_LABELS, DEFAULT_PAGE_SIZE } from '../../../core/constants';

interface EntryLineForm {
  presentationId: string;
  containerCount: number;
  unitsPerContainer: number;
  nominalSizePerUnit: number;
  costPerUnit: number;
}

@Component({
  selector: 'app-stock-entries',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RdCurrencyPipe],
  templateUrl: './stock-entries.html',
})
export class StockEntries implements OnInit {
  private inventory = inject(InventoryService);
  private fundService = inject(MonetaryFundService);
  private fb = inject(FormBuilder);

  entries = signal<StockEntryDto[]>([]);
  presentations = signal<ProductPresentationDto[]>([]);
  funds = signal<MonetaryFundDto[]>([]);
  loading = signal(true);
  saving = signal(false);
  errorMsg = signal<string | null>(null);
  successMsg = signal<string | null>(null);
  showModal = signal(false);

  page = signal(1);
  totalCount = signal(0);
  totalPages = signal(0);

  entryForm = this.fb.group({
    purchasedAt: [new Date().toISOString().split('T')[0]],
    supplierName: [''],
    notes: [''],
    fundId: [''],
    lines: this.fb.nonNullable.array<EntryLineForm>([]),
  });

  lineForms = signal<EntryLineForm[]>([]);

  totalEntryCost = computed(() =>
    this.lineForms().reduce((s, l) => {
      const units = l.containerCount * l.unitsPerContainer;
      return s + units * l.costPerUnit;
    }, 0)
  );

  ngOnInit(): void {
    this.loadEntries();
    this.loadPresentations();
    this.loadFunds();
  }

  loadEntries(): void {
    this.loading.set(true);
    this.inventory.getStockEntries(this.page(), DEFAULT_PAGE_SIZE).subscribe({
      next: res => {
        this.entries.set(res.items);
        this.totalCount.set(res.totalCount);
        this.totalPages.set(res.totalPages);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  loadPresentations(): void {
    this.inventory.getCatalog().subscribe({
      next: products => {
        const allPresentations: ProductPresentationDto[] = [];
        for (const p of products) {
          for (const pres of (p.presentations ?? [])) {
            if (pres.isActive) {
              (pres as any).productName = p.name;
              allPresentations.push(pres);
            }
          }
        }
        this.presentations.set(allPresentations);
      },
      error: () => {},
    });
  }

  loadFunds(): void {
    this.fundService.getFunds().subscribe({
      next: funds => this.funds.set(funds),
      error: () => {},
    });
  }

  openCreate(): void {
    this.errorMsg.set(null);
    this.successMsg.set(null);
    this.lineForms.set([]);
    this.entryForm.reset({
      purchasedAt: new Date().toISOString().split('T')[0],
      supplierName: '',
      notes: '',
      fundId: '',
    });
    this.showModal.set(true);
  }

  closeModal(): void {
    this.showModal.set(false);
  }

  addLine(): void {
    this.lineForms.update(lines => [...lines, {
      presentationId: '',
      containerCount: 1,
      unitsPerContainer: 1,
      nominalSizePerUnit: 1,
      costPerUnit: 0,
    }]);
  }

  removeLine(index: number): void {
    this.lineForms.update(lines => lines.filter((_, i) => i !== index));
  }

  updateLine(index: number, field: keyof EntryLineForm, value: string | number): void {
    this.lineForms.update(lines => {
      const updated = [...lines];
      updated[index] = { ...updated[index], [field]: value };
      return updated;
    });
  }

  onPresentationChange(index: number, event: Event): void {
    const value = (event.target as HTMLSelectElement).value;
    this.updateLine(index, 'presentationId', value);
  }

  onNumberInput(index: number, field: 'containerCount' | 'unitsPerContainer' | 'costPerUnit', event: Event): void {
    const value = +(event.target as HTMLInputElement).value;
    this.updateLine(index, field, value);
  }

  getPresentationName(id: string): string {
    const p = this.presentations().find(p => p.id === id);
    if (!p) return '';
    return `${(p as any).productName ?? ''} — ${p.displayName}`;
  }

  getLineTotal(line: EntryLineForm): number {
    return line.containerCount * line.unitsPerContainer * line.costPerUnit;
  }

  submit(): void {
    const lines = this.lineForms();
    if (lines.length === 0) {
      this.errorMsg.set('Agrega al menos una línea.');
      return;
    }
    for (const l of lines) {
      if (!l.presentationId || l.containerCount <= 0 || l.costPerUnit <= 0) {
        this.errorMsg.set('Completa todos los campos de cada línea.');
        return;
      }
    }

    this.saving.set(true);
    this.errorMsg.set(null);
    const v = this.entryForm.getRawValue();

    this.inventory.confirmStockEntry({
      purchasedAt: v.purchasedAt ? new Date(v.purchasedAt).toISOString() : undefined,
      supplierName: v.supplierName || null,
      notes: v.notes || null,
      fundId: v.fundId || null,
      lines: lines.map(l => ({
        presentationId: l.presentationId,
        containerCount: l.containerCount,
        unitsPerContainer: l.unitsPerContainer,
        nominalSizePerUnit: l.nominalSizePerUnit,
        costPerUnit: l.costPerUnit,
      })),
    }).subscribe({
      next: () => {
        this.saving.set(false);
        this.successMsg.set('Entrada de stock registrada exitosamente.');
        this.closeModal();
        this.loadEntries();
      },
      error: (err) => {
        this.saving.set(false);
        this.errorMsg.set(err?.error?.detail ?? err?.error?.message ?? 'Error registrando entrada.');
      },
    });
  }

  nextPage(): void {
    if (this.page() < this.totalPages()) { this.page.update(p => p + 1); this.loadEntries(); }
  }

  prevPage(): void {
    if (this.page() > 1) { this.page.update(p => p - 1); this.loadEntries(); }
  }

  presentationTypeLabel(type: number): string {
    return PRESENTATION_TYPE_LABELS[type] ?? String(type);
  }

  unitLabel(unit: number): string {
    return UNIT_OF_MEASURE_LABELS[unit] ?? String(unit);
  }

  fmtDate(d: string): string {
    return new Intl.DateTimeFormat('es-DO', { day: '2-digit', month: 'short', year: 'numeric' }).format(new Date(d));
  }
}
