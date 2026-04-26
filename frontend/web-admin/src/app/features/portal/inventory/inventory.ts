import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import {
  InventoryService,
  ProductDto,
  CreateProductRequest,
  AdjustStockRequest,
} from '../../../core/services/inventory.service';

type ModalMode = 'create' | 'edit-price' | 'adjust-stock' | null;

@Component({
  selector: 'app-inventory',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './inventory.html',
})
export class Inventory implements OnInit {
  private inventoryService = inject(InventoryService);
  private fb = inject(FormBuilder);

  // State
  products = signal<ProductDto[]>([]);
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
    categoryId: ['00000000-0000-0000-0000-000000000001', Validators.required],
  });

  priceForm = this.fb.nonNullable.group({
    newCostPrice: [0, [Validators.required, Validators.min(0)]],
    newSalePrice: [0, [Validators.required, Validators.min(0.01)]],
  });

  stockForm = this.fb.nonNullable.group({
    delta: [0, [Validators.required]],
    reason: ['', [Validators.required, Validators.minLength(3)]],
  });

  // Computed stats
  totalProducts = computed(() => this.totalCount());
  lowStockProducts = computed(() => this.products().filter(p => p.stockQuantity <= 5).length);
  activeProducts = computed(() => this.products().filter(p => p.isActive).length);

  ngOnInit(): void {
    this.loadProducts();
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
    this.createForm.reset({ itbisRate: 0.18, unitType: 1, categoryId: '00000000-0000-0000-0000-000000000001' });
    this.selectedProduct.set(null);
    this.modalMode.set('create');
    this.errorMsg.set(null);
  }

  openEditPrice(product: ProductDto): void {
    this.selectedProduct.set(product);
    this.priceForm.reset({ newCostPrice: product.costPrice, newSalePrice: product.salePrice });
    this.modalMode.set('edit-price');
    this.errorMsg.set(null);
  }

  openAdjustStock(product: ProductDto): void {
    this.selectedProduct.set(product);
    this.stockForm.reset({ delta: 0, reason: '' });
    this.modalMode.set('adjust-stock');
    this.errorMsg.set(null);
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
      error: (err) => { this.saving.set(false); this.errorMsg.set(err?.error?.message || 'Error creando producto.'); }
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
    if (this.stockForm.invalid || !this.selectedProduct()) return;
    this.saving.set(true);
    const v = this.stockForm.getRawValue();
    const req: AdjustStockRequest = { delta: v.delta, reason: v.reason };
    this.inventoryService.adjustStock(this.selectedProduct()!.productId, req).subscribe({
      next: () => { this.saving.set(false); this.closeModal(); this.loadProducts(); },
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
    { id: 3, name: 'Kilogramo' },
    { id: 4, name: 'Litro' },
    { id: 5, name: 'Galón' },
    { id: 6, name: 'Caja' },
    { id: 7, name: 'Docena' },
  ];
}
