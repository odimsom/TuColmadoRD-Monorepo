import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { forkJoin } from 'rxjs';
import { InventoryService } from '../../../core/services/inventory.service';
import { RdCurrencyPipe } from '../../../core/pipes';
import type { ProductPresentationDto, StockContainerDto, CategoryDto } from '../../../core/services/inventory.service';
import {
  PRESENTATION_TYPE, PRESENTATION_TYPE_LABELS,
  SELL_MODE, SELL_MODE_LABELS,
  UNIT_OF_MEASURE, UNIT_OF_MEASURE_LABELS,
  CONTAINER_STATUS, CONTAINER_STATUS_LABELS,
} from '../../../core/constants';

@Component({
  selector: 'app-presentations',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RdCurrencyPipe],
  templateUrl: './presentations.html',
})
export class Presentations implements OnInit {
  private inventory = inject(InventoryService);
  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);

  productId = signal<string>('');
  productName = signal<string>('');
  presentations = signal<ProductPresentationDto[]>([]);
  loading = signal(true);
  saving = signal(false);
  errorMsg = signal<string | null>(null);
  successMsg = signal<string | null>(null);
  showModal = signal(false);
  selectedPresentation = signal<ProductPresentationDto | null>(null);
  containers = signal<StockContainerDto[]>([]);
  loadingContainers = signal(false);

  readonly presentationTypes = Object.entries(PRESENTATION_TYPE).map(([key, value]) => ({
    key, value, label: PRESENTATION_TYPE_LABELS[value as number]
  }));

  readonly sellModes = Object.entries(SELL_MODE).map(([key, value]) => ({
    key, value, label: SELL_MODE_LABELS[value as number]
  }));

  readonly unitsOfMeasure = Object.entries(UNIT_OF_MEASURE).map(([key, value]) => ({
    key, value, label: UNIT_OF_MEASURE_LABELS[value as number]
  }));

  form = this.fb.group({
    displayName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(80)]],
    presentationType: [2 as number, Validators.required],
    sellMode: [1 as number, Validators.required],
    brand: [''],
    nominalCapacity: [0],
    measureUnit: [1 as number, Validators.required],
    salePrice: [0, [Validators.required, Validators.min(0.01)]],
    costPrice: [0, [Validators.required, Validators.min(0)]],
  });

  isBulkContainer = computed(() => this.form.controls.presentationType.value === 1);

  totalPresentations = computed(() => this.presentations().length);
  activePresentations = computed(() => this.presentations().filter(p => p.isActive).length);
  totalPackagedStock = computed(() => this.presentations().reduce((s, p) => s + p.packagedStockQuantity, 0));
  totalOpenContainers = computed(() => this.presentations().reduce((s, p) => s + p.openContainersCount, 0));

  ngOnInit(): void {
    this.productId.set(this.route.snapshot.paramMap.get('id') ?? '');
    this.load();
  }

  load(): void {
    this.loading.set(true);
    forkJoin({
      product: this.inventory.getProductById(this.productId()),
      presentations: this.inventory.getPresentations(this.productId()),
    }).subscribe({
      next: ({ product, presentations }) => {
        this.productName.set(product.name);
        this.presentations.set(presentations);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  openCreate(): void {
    this.selectedPresentation.set(null);
    this.errorMsg.set(null);
    this.successMsg.set(null);
    this.form.reset({
      displayName: '',
      presentationType: 2,
      sellMode: 1,
      brand: '',
      nominalCapacity: 0,
      measureUnit: 1,
      salePrice: 0,
      costPrice: 0,
    });
    this.showModal.set(true);
  }

  openEdit(p: ProductPresentationDto): void {
    this.selectedPresentation.set(p);
    this.errorMsg.set(null);
    this.successMsg.set(null);
    this.form.patchValue({
      displayName: p.displayName,
      presentationType: p.presentationType,
      sellMode: p.sellMode,
      brand: p.brand ?? '',
      nominalCapacity: p.nominalCapacity ?? 0,
      measureUnit: p.measureUnit,
      salePrice: p.salePrice,
      costPrice: p.costPrice,
    });
    this.showModal.set(true);
  }

  closeModal(): void {
    this.showModal.set(false);
    this.selectedPresentation.set(null);
  }

  submit(): void {
    if (this.form.invalid || this.saving()) { this.form.markAllAsTouched(); return; }
    this.saving.set(true);
    this.errorMsg.set(null);
    const v = this.form.getRawValue();
    const pt = v.presentationType ?? 2;
    const cmd = {
      displayName: v.displayName ?? '',
      presentationType: pt,
      sellMode: v.sellMode ?? 1,
      brand: v.brand || null,
      nominalCapacity: pt === 1 ? (v.nominalCapacity ?? null) : null,
      measureUnit: v.measureUnit ?? 1,
      salePrice: v.salePrice ?? 0,
      costPrice: v.costPrice ?? 0,
    };

    this.inventory.addPresentation(this.productId(), cmd).subscribe({
      next: () => {
        this.saving.set(false);
        this.successMsg.set(`Presentación "${v.displayName}" creada.`);
        this.closeModal();
        this.load();
      },
      error: (err) => {
        this.saving.set(false);
        this.errorMsg.set(err?.error?.detail ?? err?.error?.message ?? 'Error al crear la presentación.');
      },
    });
  }

  deactivate(p: ProductPresentationDto): void {
    if (!confirm(`¿Desactivar "${p.displayName}"?`)) return;
    this.inventory.deactivatePresentation(p.id).subscribe({
      next: () => { this.successMsg.set('Presentación desactivada.'); this.load(); },
      error: (err) => this.errorMsg.set(err?.error?.message ?? 'Error desactivando.'),
    });
  }

  viewContainers(p: ProductPresentationDto): void {
    this.selectedPresentation.set(p);
    this.loadingContainers.set(true);
    this.inventory.getContainersByPresentation(p.id).subscribe({
      next: containers => { this.containers.set(containers); this.loadingContainers.set(false); },
      error: () => this.loadingContainers.set(false),
    });
  }

  hideContainers(): void {
    this.containers.set([]);
    this.selectedPresentation.set(null);
  }

  openContainer(containerId: string): void {
    this.inventory.openContainer(containerId, {}).subscribe({
      next: () => { this.successMsg.set('Contenedor abierto.'); this.loadContainersForSelected(); },
      error: (err) => this.errorMsg.set(err?.error?.message ?? 'Error abriendo contenedor.'),
    });
  }

  markContainerEmpty(containerId: string): void {
    if (!confirm('¿Marcar este contenedor como vacío?')) return;
    this.inventory.markContainerEmpty(containerId).subscribe({
      next: () => { this.successMsg.set('Contenedor marcado como vacío.'); this.loadContainersForSelected(); },
      error: (err) => this.errorMsg.set(err?.error?.message ?? 'Error marcando vacío.'),
    });
  }

  private loadContainersForSelected(): void {
    const p = this.selectedPresentation();
    if (p) this.viewContainers(p);
  }

  presentationTypeLabel(type: number): string {
    return PRESENTATION_TYPE_LABELS[type] ?? String(type);
  }

  sellModeLabel(mode: number): string {
    return SELL_MODE_LABELS[mode] ?? String(mode);
  }

  unitLabel(unit: number): string {
    return UNIT_OF_MEASURE_LABELS[unit] ?? String(unit);
  }

  containerStatusLabel(status: number): string {
    return CONTAINER_STATUS_LABELS[status] ?? String(status);
  }

  containerStatusColor(status: number): string {
    switch (status) {
      case CONTAINER_STATUS.SEALED: return 'text-slate-400 bg-slate-500/10 ring-slate-500/20';
      case CONTAINER_STATUS.OPEN: return 'text-green-400 bg-green-500/10 ring-green-500/20';
      case CONTAINER_STATUS.EMPTY: return 'text-red-400 bg-red-500/10 ring-red-500/20';
      default: return 'text-slate-400';
    }
  }

  fmtDate(d: string): string {
    return new Intl.DateTimeFormat('es-DO', { day: '2-digit', month: 'short', year: 'numeric' }).format(new Date(d));
  }
}
