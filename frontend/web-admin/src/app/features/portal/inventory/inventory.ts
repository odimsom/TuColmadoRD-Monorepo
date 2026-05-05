import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import {
  InventoryService,
  ProductDto,
  CreateProductRequest,
  AdjustStockRequest,
  CategoryDto,
} from '../../../core/services/inventory.service';
import { ExpenseService } from '../../../core/services/expense.service';
import { RdCurrencyPipe } from '../../../core/pipes';

type ModalMode = 'create' | 'edit-price' | 'adjust-stock' | null;

function salePriceValidator(group: AbstractControl): ValidationErrors | null {
  const cost = group.get('costPrice')?.value ?? 0;
  const sale = group.get('salePrice')?.value ?? 0;
  return sale > 0 && sale < cost ? { saleBelowCost: true } : null;
}

const ERROR_MESSAGES: Record<string, string> = {
  'product.sale_price_below_cost': 'El precio de venta no puede ser menor al costo.',
  'product.category_required': 'Selecciona una categoría.',
  'product.name_required': 'El nombre del producto es obligatorio.',
  'product.invalid_price': 'El precio ingresado no es válido.',
  'category.not_found': 'La categoría seleccionada no existe.',
};

function mapInventoryError(code?: string): string {
  if (!code) return '';
  return ERROR_MESSAGES[code] ?? code;
}

@Component({
  selector: 'app-inventory',
  standalone: true,
  imports: [CommonModule, RouterLink, ReactiveFormsModule, RdCurrencyPipe],
  templateUrl: './inventory.html',
})
export class Inventory implements OnInit {
  private inventoryService = inject(InventoryService);
  private expenseService = inject(ExpenseService);
  private fb = inject(FormBuilder);

  // State
  products = signal<ProductDto[]>([]);
  categories = signal<CategoryDto[]>([]);
  loading = signal(true);
  saving = signal(false);
  errorMsg = signal<string | null>(null);

  // Pagination
  page = signal(1);
  pageSize = signal(20);
  totalCount = signal(0);
  totalPages = signal(0);
  searchQuery = signal('');

  // Modal
  modalMode = signal<ModalMode>(null);
  selectedProduct = signal<ProductDto | null>(null);

  // Forms
  createForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(120)]],
    costPrice: [0, [Validators.required, Validators.min(0)]],
    salePrice: [0, [Validators.required, Validators.min(0.01)]],
    itbisRate: [0.18, [Validators.required, Validators.min(0), Validators.max(1)]],
    unitType: [1, Validators.required],
    categoryId: ['', Validators.required],
  }, { validators: salePriceValidator });

  priceForm = this.fb.nonNullable.group({
    newCostPrice: [0, [Validators.required, Validators.min(0)]],
    newSalePrice: [0, [Validators.required, Validators.min(0.01)]],
  });

  adjustType = signal<'compra' | 'ajuste'>('ajuste');

  stockForm = this.fb.nonNullable.group({
    delta: [0, [Validators.required, Validators.min(0.001)]],
    reason: [''],
    expenseAmount: [0, [Validators.min(0)]],
  });

  calculatedExpense = computed(() => {
    const product = this.selectedProduct();
    const delta = this.stockForm.controls.delta.value ?? 0;
    if (!product || this.adjustType() !== 'compra' || delta <= 0) return 0;
    return Math.round(delta * product.costPrice * 100) / 100;
  });

  // Computed stats
  totalProducts = computed(() => this.totalCount());
  lowStockProducts = computed(() => this.products().filter(p => p.stockQuantity <= 5).length);
  activeProducts = computed(() => this.products().filter(p => p.isActive).length);

  ngOnInit(): void {
    this.loadProducts();
    this.loadCategories();
  }

  loadingCategories = signal(false);

  loadCategories(): void {
    this.loadingCategories.set(true);
    this.inventoryService.getCategories().subscribe({
      next: (cats) => { this.categories.set(cats); this.loadingCategories.set(false); },
      error: () => this.loadingCategories.set(false),
    });
  }

  loadProducts(): void {
    this.loading.set(true);
    this.errorMsg.set(null);
    this.inventoryService.getProducts(this.page(), this.pageSize(), this.searchQuery() || undefined).subscribe({
      next: (res) => {
        this.products.set(res.items);
        this.totalCount.set(res.totalCount);
        this.totalPages.set(res.totalPages);
        this.loading.set(false);
      },
      error: (err) => {
        this.errorMsg.set(err?.error?.message || 'Error cargando productos.');
        this.loading.set(false);
      }
    });
  }

  onSearch(event: Event): void {
    const val = (event.target as HTMLInputElement).value;
    this.searchQuery.set(val);
    this.page.set(1);
    this.loadProducts();
  }

  nextPage(): void {
    if (this.page() < this.totalPages()) {
      this.page.update(p => p + 1);
      this.loadProducts();
    }
  }

  prevPage(): void {
    if (this.page() > 1) {
      this.page.update(p => p - 1);
      this.loadProducts();
    }
  }

  // --- Modal openers ---
  openCreate(): void {
    this.selectedProduct.set(null);
    this.errorMsg.set(null);
    this.modalMode.set('create');
    this.loadingCategories.set(true);
    this.inventoryService.getCategories().subscribe({
      next: (cats) => {
        this.categories.set(cats);
        this.loadingCategories.set(false);
        this.createForm.reset({ itbisRate: 0.18, unitType: 1, categoryId: cats[0]?.id ?? '' });
      },
      error: () => this.loadingCategories.set(false),
    });
  }

  openEditPrice(product: ProductDto): void {
    this.selectedProduct.set(product);
    this.priceForm.reset({ newCostPrice: product.costPrice, newSalePrice: product.salePrice });
    this.modalMode.set('edit-price');
    this.errorMsg.set(null);
  }

  openAdjustStock(product: ProductDto): void {
    this.selectedProduct.set(product);
    this.adjustType.set('ajuste');
    this.stockForm.reset({ delta: 0, reason: '', expenseAmount: 0 });
    this.modalMode.set('adjust-stock');
    this.errorMsg.set(null);
  }

  setAdjustType(type: 'compra' | 'ajuste'): void {
    this.adjustType.set(type);
    this.stockForm.patchValue({ reason: '', expenseAmount: 0 });
  }

  closeModal(): void {
    this.modalMode.set(null);
    this.selectedProduct.set(null);
    this.errorMsg.set(null);
  }

  // --- Submit handlers ---
  submitCreate(): void {
    if (this.createForm.invalid) { this.createForm.markAllAsTouched(); return; }
    this.saving.set(true);
    const v = this.createForm.getRawValue();
    const cmd: CreateProductRequest = {
      name: v.name,
      categoryId: v.categoryId,
      costPrice: v.costPrice,
      salePrice: v.salePrice,
      itbisRate: v.itbisRate,
      unitType: v.unitType,
    };
    this.inventoryService.createProduct(cmd).subscribe({
      next: () => { this.saving.set(false); this.closeModal(); this.loadProducts(); },
      error: (err) => { this.saving.set(false); this.errorMsg.set(mapInventoryError(err?.error?.error ?? err?.error?.message) || 'Error creando producto.'); }
    });
  }

  submitPrice(): void {
    if (this.priceForm.invalid || !this.selectedProduct()) return;
    this.saving.set(true);
    const v = this.priceForm.getRawValue();
    this.inventoryService.updatePrice(this.selectedProduct()!.productId, v).subscribe({
      next: () => { this.saving.set(false); this.closeModal(); this.loadProducts(); },
      error: (err) => { this.saving.set(false); this.errorMsg.set(err?.error?.message || 'Error actualizando precio.'); }
    });
  }

  submitStock(): void {
    const product = this.selectedProduct();
    if (!product) return;
    const v = this.stockForm.getRawValue();
    if (v.delta <= 0 && this.adjustType() === 'compra') {
      this.errorMsg.set('La cantidad debe ser mayor a 0 para registrar una compra.');
      return;
    }
    if (this.adjustType() === 'ajuste' && !v.reason.trim()) {
      this.stockForm.controls.reason.setErrors({ required: true });
      this.stockForm.markAllAsTouched();
      return;
    }
    this.saving.set(true);
    const reason = this.adjustType() === 'compra'
      ? `Compra de ${v.delta} ${this.unitTypeLabel(product.unitTypeId)} de ${product.name}`
      : v.reason;
    const req: AdjustStockRequest = { delta: v.delta, reason };
    this.inventoryService.adjustStock(product.productId, req).subscribe({
      next: () => {
        if (this.adjustType() === 'compra') {
          const amount = v.expenseAmount > 0 ? v.expenseAmount : this.calculatedExpense();
          this.expenseService.registerExpense({
            amount,
            category: 'Compra de Inventario',
            description: reason,
          }).subscribe({
            next: () => { this.saving.set(false); this.closeModal(); this.loadProducts(); },
            error: () => { this.saving.set(false); this.closeModal(); this.loadProducts(); },
          });
        } else {
          this.saving.set(false); this.closeModal(); this.loadProducts();
        }
      },
      error: (err) => { this.saving.set(false); this.errorMsg.set(err?.error?.message || 'Error ajustando stock.'); }
    });
  }

  deactivate(product: ProductDto): void {
    if (!confirm(`¿Desactivar "${product.name}"? Ya no aparecerá en ventas.`)) return;
    this.inventoryService.deactivateProduct(product.productId).subscribe({
      next: () => this.loadProducts(),
      error: (err) => alert(err?.error?.message || 'Error desactivando producto.')
    });
  }

  getStockClass(qty: number): string {
    if (qty <= 0) return 'text-red-500';
    if (qty <= 5) return 'text-amber-400';
    return 'text-green-400';
  }

  readonly unitTypes = [
    { id: 1, name: 'Unidad' },
    { id: 2, name: 'Libra' },
    { id: 3, name: 'Litro' },
    { id: 4, name: 'Caja' },
  ];

  private static readonly UNIT_LABELS: Record<number, string> = {
    1: 'Unidad',
    2: 'Libra',
    3: 'Litro',
    4: 'Caja',
  };

  unitTypeLabel(id: number): string {
    return Inventory.UNIT_LABELS[id] ?? 'Unidad';
  }
}
