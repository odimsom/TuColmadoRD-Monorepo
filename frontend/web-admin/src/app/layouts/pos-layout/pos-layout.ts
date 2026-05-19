import { Component, signal, computed, inject, OnInit, OnDestroy, HostListener, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import {
  SaleService, ShiftDto, CreateSaleResult
} from '../../core/services/sale.service';
import { InventoryService, ProductDto } from '../../core/services/inventory.service';
import { CustomerService, CustomerSummary } from '../../core/services/customer.service';
import { SettingsService, TenantProfileDto } from '../../core/services/settings.service';
import { RdCurrencyPipe, RncPipe } from '../../core/pipes';
import { PAYMENT_METHOD as PM, UNIT_TYPE, LOW_STOCK_THRESHOLD, CATALOG_PAGE_SIZE, PRODUCT_COLORS, NOMINATIM_BASE_URL, DR_LOCALE, DR_CURRENCY_CODE, WHATSAPP_BASE_URL, DEFAULT_CREDIT_LIMIT } from '../../core/constants';

export interface CartItem {
  productId: string;
  presentationId: string;
  name: string;
  displayName: string;
  salePrice: number;
  itbisRate: number;
  unitTypeId: number;
  quantity: number;
  lineSubtotal: number;
  lineItbis: number;
  lineTotal: number;
}

export interface CatalogPresentation {
  productId: string;
  productName: string;
  presentationId: string;
  displayName: string;
  salePrice: number;
  itbisRate: number;
  unitTypeId: number;
  stockQuantity: number;
  categoryName: string;
  presentationType: number;
  sellMode: number;
  isActive: boolean;
}

@Component({
  selector: 'app-pos-layout',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RdCurrencyPipe, RncPipe],
  templateUrl: './pos-layout.html',
  styleUrl: './pos-layout.scss',
})
export class PosLayout implements OnInit, OnDestroy {
  private auth      = inject(AuthService);
  private sales     = inject(SaleService);
  private inv       = inject(InventoryService);
  private customers = inject(CustomerService);
  private settings  = inject(SettingsService);
  private router    = inject(Router);
  private fb        = inject(FormBuilder);

  // ── Exposed for template ──────────────────────────
  authService = this.auth;

  @ViewChild('quickSaleAmountInput') quickSaleAmountInput?: ElementRef<HTMLInputElement>;
  @ViewChild('cashTenderedInput') cashTenderedInput?: ElementRef<HTMLInputElement>;

  // ── Data signals ─────────────────────────────────
  catalog       = signal<CatalogPresentation[]>([]);
  cartItems     = signal<CartItem[]>([]);
  activeShift   = signal<ShiftDto | null>(null);
  lastSale      = signal<CreateSaleResult | null>(null);
  lastSalePaymentMethod = signal<string>('');
  lastSaleCustomerName  = signal<string>('');
  lastSaleCustomerPhone = signal<string>('');
  tenantProfile = signal<TenantProfileDto | null>(null);
  allCustomers  = signal<CustomerSummary[]>([]);

  // ── UI state ─────────────────────────────────────
  mobileView          = signal<'catalog' | 'cart'>('catalog');
  loading             = signal(false);
  saving              = signal(false);
  catalogSearch       = signal('');
  selectedCategory    = signal<string | null>(null);
  showOpenShiftModal   = signal(false);
  showCloseShiftModal  = signal(false);
  showPaymentModal     = signal(false);
  showReceiptModal     = signal(false);
  showAddCustomerModal = signal(false);
  showQuickSaleModal   = signal(false);
  errorMsg            = signal<string | null>(null);

  // ── Payment state ─────────────────────────────────
  paymentMethod  = signal<'cash' | 'card' | 'transfer' | 'credit' | 'delivery'>('cash');
  cashTendered   = signal(0);
  buyerRnc       = signal('');
  selectedCustomer = signal<CustomerSummary | null>(null);
  customerSearchQuery = signal('');

  // ── Geocoding (Nominatim) ─────────────────────────
  geocodeLat     = signal<number | null>(null);
  geocodeLon     = signal<number | null>(null);
  geocodeLoading = signal(false);
  geocodeError   = signal<string | null>(null);

  // ── Quick Sale ────────────────────────────────────
  quickSaleSearch = signal('');
  quickSaleAmount = signal<number | null>(null);
  selectedQuickProduct = signal<CatalogPresentation | null>(null);

  // ── Catalog pagination ────────────────────────────
  readonly PAGE_SIZE = CATALOG_PAGE_SIZE;
  catalogPage = signal(0);

  @HostListener('window:keydown', ['$event'])
  handleKeyboardEvent(event: KeyboardEvent) {
    if (event.key === 'F2') {
      event.preventDefault();
      if (this.showQuickSaleModal()) {
         this.closeModals();
      } else {
         this.openQuickSaleModal();
      }
    } else if (event.key === 'F4') {
      event.preventDefault();
      if (!this.showPaymentModal()) this.openPaymentModal();
    } else if (event.key === 'F8') {
      event.preventDefault();
      if (this.showReceiptModal()) this.printReceipt();
    } else if (event.key === 'F9') {
      event.preventDefault();
      if (this.showPaymentModal()) this.submitSale();
    } else if (event.key === 'Escape') {
      this.closeModals();
    }
  }

  closeModals(): void {
    this.showQuickSaleModal.set(false);
    this.showPaymentModal.set(false);
    this.showReceiptModal.set(false);
    this.showAddCustomerModal.set(false);
    this.showOpenShiftModal.set(false);
    this.showCloseShiftModal.set(false);
  }

  openQuickSaleModal(preselect?: CatalogPresentation): void {
    if (!this.activeShift()) {
      this.showOpenShiftModal.set(true);
      return;
    }
    this.showQuickSaleModal.set(true);
    this.quickSaleAmount.set(null);
    this.quickSaleSearch.set('');
    this.selectedQuickProduct.set(preselect ?? null);
    setTimeout(() => this.quickSaleAmountInput?.nativeElement?.focus(), 100);
  }

  topQuickProducts = computed(() => {
    const q = this.quickSaleSearch().toLowerCase().trim();
    const sorted = [...this.catalog()].sort((a, b) => b.stockQuantity - a.stockQuantity);
    if (!q) return sorted.slice(0, 8);
    return sorted.filter(p => p.displayName.toLowerCase().includes(q) || p.productName.toLowerCase().includes(q)).slice(0, 8);
  });

  submitQuickSale(): void {
    const p = this.selectedQuickProduct();
    const amt = this.quickSaleAmount();
    if (!p || !amt || amt <= 0) return;

    // Calculate quantity based on the amount the user wants to pay
    // Price = SalePrice + (SalePrice * ItbisRate) = SalePrice * (1 + ItbisRate)
    const unitPriceWithTax = p.salePrice * (1 + p.itbisRate);
    const quantity = amt / unitPriceWithTax;

    this.addToCart(p, quantity);
    this.closeModals();
  }

  // ── Clock ─────────────────────────────────────────
  clock = signal(new Date());
  private clockTimer?: ReturnType<typeof setInterval>;

  // ── Forms ─────────────────────────────────────────
  openShiftForm = this.fb.group({
    cashierName:       ['', [Validators.required, Validators.minLength(3)]],
    openingCashAmount: [0,  [Validators.required, Validators.min(0)]]
  });

  closeShiftForm = this.fb.group({
    actualCashAmount: [0, [Validators.required, Validators.min(0)]],
    notes: ['']
  });

  customerForm = this.fb.group({
    fullName: ['', [Validators.required, Validators.minLength(3)]],
    phone:    [''],
    documentId: ['', [Validators.required]],
    creditLimit: [DEFAULT_CREDIT_LIMIT, [Validators.required, Validators.min(0)]],
    province: [''],
    sector: [''],
    street: [''],
    houseNumber: [''],
    reference: ['']
  });

  deliveryAddressForm = this.fb.group({
    province: ['', Validators.required],
    sector: ['', Validators.required],
    street: ['', Validators.required],
    houseNumber: [''],
    reference: ['', Validators.required]
  });

  expectedCash = computed(() => {
    const s = this.activeShift();
    if (!s) return 0;
    return s.openingCashAmount + s.totalSalesAmount;
  });

  // ── Computed ──────────────────────────────────────
  filteredCatalog = computed(() => {
    let items = this.catalog();
    const q   = this.catalogSearch().toLowerCase().trim();
    const cat = this.selectedCategory();
    if (q)   items = items.filter(p => p.displayName.toLowerCase().includes(q) || p.productName.toLowerCase().includes(q));
    if (cat) items = items.filter(p => p.categoryName === cat);
    return items;
  });

  categories = computed(() => {
    const seen = new Map<string, string>();
    for (const p of this.catalog()) {
      if (!seen.has(p.productName + '|' + p.categoryName)) {
        const key = p.categoryName;
        if (!seen.has(key)) seen.set(key, key);
      }
    }
    return Array.from(seen.entries()).map(([id, name]) => ({ id, name }));
  });

  filteredCustomers = computed(() => {
    const q = this.customerSearchQuery().toLowerCase().trim();
    if (!q) return this.allCustomers().slice(0, 10);
    return this.allCustomers().filter(c => 
      c.fullName.toLowerCase().includes(q) || 
      c.phone?.includes(q)
    ).slice(0, 10);
  });

  cartSubtotal = computed(() => this.cartItems().reduce((s, i) => s + i.lineSubtotal, 0));
  cartItbis    = computed(() => this.cartItems().reduce((s, i) => s + i.lineItbis, 0));
  cartTotal    = computed(() => this.cartItems().reduce((s, i) => s + i.lineTotal, 0));
  cartCount    = computed(() => this.cartItems().reduce((s, i) => s + i.quantity, 0));

  changeDue = computed(() => {
    if (this.paymentMethod() === 'cash') {
      return Math.max(0, this.cashTendered() - this.cartTotal());
    }
    return 0;
  });

  whatsAppDeliveryUrl = computed(() => {
    const sale = this.lastSale();
    const phone = this.lastSaleCustomerPhone().replace(/\D/g, '');
    const name  = this.lastSaleCustomerName();
    if (!sale?.confirmationCode || !phone) return null;
    const total = new Intl.NumberFormat(DR_LOCALE, { style: 'currency', currency: DR_CURRENCY_CODE }).format(sale.total);
    const text  = encodeURIComponent(
      `Hola ${name}! Tu pedido de ${total} está en camino. Tu código de confirmación de entrega es: *${sale.confirmationCode}*. Dáselo al repartidor cuando recibas el pedido.`
    );
    return `${WHATSAPP_BASE_URL}/${phone}?text=${text}`;
  });

  itbisBreakdown = computed(() => {
    const m = new Map<number, number>();
    for (const item of this.cartItems()) {
      const r = item.itbisRate;
      if (r > 0) m.set(r, (m.get(r) ?? 0) + item.lineItbis);
    }
    return Array.from(m.entries()).map(([rate, amount]) => ({
      label:  `ITBIS ${(rate * 100).toFixed(0)}%`,
      amount
    }));
  });

  // ── Lifecycle ─────────────────────────────────────
  ngOnInit(): void {
    this.loadCatalog();
    this.loadShift();
    this.loadTenantProfile();
    this.loadCustomers();
    this.clockTimer = setInterval(() => this.clock.set(new Date()), 1000);
    // Pre-fill cashier name
    const u = this.auth.currentUser();
    if (u) {
      const name = `${u.firstName ?? ''} ${u.lastName ?? ''}`.trim() || u.email;
      this.openShiftForm.patchValue({ cashierName: name });
    }
  }

  ngOnDestroy(): void {
    if (this.clockTimer) clearInterval(this.clockTimer);
  }

  // ── Data loading ──────────────────────────────────
  loadCatalog(): void {
    this.loading.set(true);
    this.inv.getCatalog().subscribe({
      next: products => {
        const presentations: CatalogPresentation[] = [];
        for (const p of products) {
          if (!p.isActive) continue;
          for (const pres of (p.presentations ?? [])) {
            if (!pres.isActive) continue;
            presentations.push({
              productId: p.productId,
              productName: p.name,
              presentationId: pres.id,
              displayName: pres.displayName,
              salePrice: pres.salePrice,
              itbisRate: p.itbisRate,
              unitTypeId: pres.measureUnit,
              stockQuantity: pres.stockQuantity,
              categoryName: p.categoryName,
              presentationType: pres.presentationType,
              sellMode: pres.sellMode,
              isActive: pres.isActive,
            });
          }
        }
        this.catalog.set(presentations);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  loadShift(): void {
    this.sales.getCurrentShift().subscribe({
      next: shift => this.activeShift.set(shift),
      error: () => this.activeShift.set(null)
    });
  }

  loadTenantProfile(): void {
    this.settings.getProfile().subscribe({
      next: profile => this.tenantProfile.set(profile),
      error: () => {} // non-critical — receipt falls back to generic header
    });
  }

  loadCustomers(): void {
    this.customers.getCustomers().subscribe({
      next: data => this.allCustomers.set(data),
      error: () => {}
    });
  }

  // ── Cart operations ───────────────────────────────
  updateStock(presentationId: string, delta: number): void {
    this.catalog.update(cats => cats.map(p =>
      p.presentationId === presentationId ? { ...p, stockQuantity: p.stockQuantity + delta } : p
    ));
  }

  addToCart(product: CatalogPresentation, customQuantity?: number): void {
    if (!this.activeShift()) {
      this.showOpenShiftModal.set(true);
      return;
    }

    if (customQuantity === undefined && (product.sellMode === 2 || product.sellMode === 3)) {
      this.openQuickSaleModal(product);
      return;
    }

    const qtyToAdd = customQuantity ?? 1;
    const items = this.cartItems();
    const idx   = items.findIndex(i => i.presentationId === product.presentationId);

    if (idx >= 0) {
      this.setQuantity(idx, items[idx].quantity + qtyToAdd);
    } else {
      const sub   = product.salePrice * qtyToAdd;
      const itbis = sub * product.itbisRate;
      this.cartItems.set([...items, {
        productId:     product.productId,
        presentationId: product.presentationId,
        name:          product.productName,
        displayName:   product.displayName,
        salePrice:     product.salePrice,
        itbisRate:     product.itbisRate,
        unitTypeId:    product.unitTypeId,
        quantity:      qtyToAdd,
        lineSubtotal:  sub,
        lineItbis:     itbis,
        lineTotal:     sub + itbis
      }]);
      this.updateStock(product.presentationId, -qtyToAdd);
    }
    this.syncCashTendered();
  }

  setQuantity(index: number, qty: number): void {
    if (qty <= 0) { this.removeItem(index); return; }
    const items   = [...this.cartItems()];
    const item    = items[index];
    
    const delta   = qty - item.quantity;
    
    const sub     = item.salePrice * qty;
    const itbis   = sub * item.itbisRate;
    items[index]  = { ...item, quantity: qty, lineSubtotal: sub, lineItbis: itbis, lineTotal: sub + itbis };
    this.cartItems.set(items);
    
    this.updateStock(item.productId, -delta);
    this.syncCashTendered();
  }

  removeItem(index: number): void {
    const item = this.cartItems()[index];
    this.updateStock(item.productId, item.quantity);
    this.cartItems.update(items => items.filter((_, i) => i !== index));
    this.syncCashTendered();
  }

  cancelSale(): void {
    for (const item of this.cartItems()) {
      this.updateStock(item.productId, item.quantity);
    }
    this.clearCart();
  }

  clearCart(): void {
    this.cartItems.set([]);
    this.cashTendered.set(0);
    this.buyerRnc.set('');
    this.selectedCustomer.set(null);
  }

  private syncCashTendered(): void {
    // Round up to nearest 100 for convenience
    const total  = this.cartTotal();
    const rounded = Math.ceil(total / 100) * 100;
    this.cashTendered.set(rounded);
  }

  // ── Shift ─────────────────────────────────────────
  submitOpenShift(): void {
    if (this.openShiftForm.invalid || this.saving()) return;
    this.saving.set(true);
    this.errorMsg.set(null);
    const v = this.openShiftForm.value;
    this.sales.openShift({
      cashierName:       v.cashierName!,
      openingCashAmount: v.openingCashAmount!
    }).subscribe({
      next: () => {
        this.showOpenShiftModal.set(false);
        this.loadShift();
        this.saving.set(false);
      },
      error: (e) => {
        this.errorMsg.set(e?.error?.detail ?? 'Error al abrir el turno.');
        this.saving.set(false);
      }
    });
  }

  openCloseShiftModal(): void {
    this.closeShiftForm.patchValue({ actualCashAmount: this.expectedCash(), notes: '' });
    this.errorMsg.set(null);
    this.showCloseShiftModal.set(true);
  }

  submitCloseShift(): void {
    const shift = this.activeShift();
    if (!shift || this.closeShiftForm.invalid || this.saving()) return;
    this.saving.set(true);
    this.errorMsg.set(null);
    const v = this.closeShiftForm.value;
    this.sales.closeShift(shift.shiftId, {
      actualCashAmount: v.actualCashAmount!,
      notes: v.notes || undefined
    }).subscribe({
      next: () => {
        this.showCloseShiftModal.set(false);
        this.activeShift.set(null);
        this.saving.set(false);
      },
      error: (e) => {
        this.errorMsg.set(e?.error?.detail ?? 'Error al cerrar el turno.');
        this.saving.set(false);
      }
    });
  }

  // ── Customers ─────────────────────────────────────
  selectCustomer(c: CustomerSummary): void {
    this.selectedCustomer.set(c);
    this.customerSearchQuery.set('');
    // Auto-geocode when selected for delivery and customer has address but no coords
    if (this.paymentMethod() === 'delivery' && c.province && c.street && c.sector && !c.latitude) {
      this.geocodeCustomerAddress(c);
    }
  }

  private async geocodeCustomerAddress(c: CustomerSummary): Promise<void> {
    const query = encodeURIComponent(
      `${c.street}${c.houseNumber ? ' ' + c.houseNumber : ''}, ${c.sector}, ${c.province}, República Dominicana`
    );
    try {
      const res  = await fetch(`${NOMINATIM_BASE_URL}?q=${query}&format=json&limit=1&countrycodes=do`);
      const data = await res.json();
      if (data?.length > 0) {
        this.selectedCustomer.update(prev => prev
          ? { ...prev, latitude: parseFloat(data[0].lat), longitude: parseFloat(data[0].lon) }
          : prev
        );
      }
    } catch { /* silent — delivery still works without GPS */ }
  }

  submitAddCustomer(): void {
    if (this.customerForm.invalid || this.saving()) return;
    this.saving.set(true);
    this.errorMsg.set(null);
    const v = this.customerForm.value;
    
    let address = null;
    if (v.province && v.sector && v.street && v.reference) {
      address = {
        province: v.province,
        sector: v.sector,
        street: v.street,
        reference: v.reference,
        houseNumber: v.houseNumber || undefined
      };
    }

    this.customers.createCustomer({
      fullName: v.fullName!,
      documentId: v.documentId!,
      phone: v.phone || null,
      creditLimit: v.creditLimit!,
      address: address
    }).subscribe({
      next: res => {
        this.loadCustomers();
        // Auto-select the newly created customer
        this.selectedCustomer.set({
          customerId: res.customerId,
          fullName: v.fullName!,
          phone: v.phone || '',
          balance: 0,
          creditLimit: v.creditLimit!,
          isActive: true,
          province: v.province,
          sector: v.sector,
          street: v.street,
          reference: v.reference,
          houseNumber: v.houseNumber || null
        });
        this.showAddCustomerModal.set(false);
        this.customerForm.reset({ creditLimit: DEFAULT_CREDIT_LIMIT });
        this.saving.set(false);
      },
      error: e => {
        this.errorMsg.set(e?.error?.detail ?? 'Error al crear cliente.');
        this.saving.set(false);
      }
    });
  }

  // ── Checkout ──────────────────────────────────────
  openPaymentModal(): void {
    if (!this.cartItems().length || !this.activeShift()) return;
    this.syncCashTendered();
    this.errorMsg.set(null);
    this.showPaymentModal.set(true);
  }

  submitSale(): void {
    if (this.saving() || !this.cartItems().length) return;
    const total   = this.cartTotal();
    
    // Validations for Credit
    if (this.paymentMethod() === 'credit' && !this.selectedCustomer()) {
      this.errorMsg.set('Debes seleccionar un cliente para ventas a crédito.');
      return;
    }

    const paid    = this.paymentMethod() === 'cash' ? this.cashTendered() : total;
    if (paid < total && this.paymentMethod() !== 'credit' && this.paymentMethod() !== 'delivery') {
      this.errorMsg.set('El monto recibido es menor al total.');
      return;
    }

    this.saving.set(true);
    this.errorMsg.set(null);
    
    const methodId = this.paymentMethod() === 'cash' ? PM.CASH
                   : this.paymentMethod() === 'card' ? PM.CARD
                   : this.paymentMethod() === 'transfer' ? PM.TRANSFER
                   : this.paymentMethod() === 'credit' ? PM.CREDIT
                   : PM.DELIVERY;

    let deliveryAddress = null;
    if (this.paymentMethod() === 'delivery') {
      const c = this.selectedCustomer();
      if (c && c.province && c.sector && c.street && c.reference) {
        deliveryAddress = {
          province: c.province,
          sector: c.sector,
          street: c.street,
          reference: c.reference,
          houseNumber: c.houseNumber || undefined,
          latitude: c.latitude || undefined,
          longitude: c.longitude || undefined
        };
      } else if (this.deliveryAddressForm.valid) {
        const v = this.deliveryAddressForm.value;
        deliveryAddress = {
          province: v.province!,
          sector: v.sector!,
          street: v.street!,
          reference: v.reference!,
          houseNumber: v.houseNumber || undefined,
          latitude: this.geocodeLat() ?? undefined,
          longitude: this.geocodeLon() ?? undefined
        };
      } else {
        this.errorMsg.set('Para delivery, selecciona un cliente con dirección o llena los datos de envío.');
        this.saving.set(false);
        return;
      }
    }

    this.sales.createSale({
      items:    this.cartItems().map(i => ({ productId: i.productId, presentationId: i.presentationId, quantity: i.quantity })),
      payments: [{ 
        paymentMethodId: methodId, 
        amount: total, // For credit/delivery, the amount registered is the total
        customerId: this.selectedCustomer()?.customerId 
      }],
      buyerRnc: this.buyerRnc() || null,
      deliveryAddress: deliveryAddress,
      notes:    this.paymentMethod() === 'credit' ? `Fiado a ${this.selectedCustomer()?.fullName}` : null
    }).subscribe({
      next: result => {
        this.lastSale.set(result);
        this.lastSalePaymentMethod.set(this.paymentMethod());
        this.lastSaleCustomerName.set(this.selectedCustomer()?.fullName ?? '');
        this.lastSaleCustomerPhone.set(this.selectedCustomer()?.phone ?? '');
        this.showPaymentModal.set(false);
        this.showReceiptModal.set(true);
        this.clearCart();
        this.saving.set(false);
      },
      error: (e) => {
        this.errorMsg.set(e?.error?.detail ?? 'Error al registrar la venta.');
        this.saving.set(false);
      }
    });
  }

  printReceipt(): void {
    window.print();
  }

  async geocodeAddress(): Promise<void> {
    const v = this.deliveryAddressForm.value;
    if (!v.street || !v.sector || !v.province) {
      this.geocodeError.set('Completa al menos calle, sector y provincia.');
      return;
    }
    this.geocodeLoading.set(true);
    this.geocodeError.set(null);
    this.geocodeLat.set(null);
    this.geocodeLon.set(null);

    const query = encodeURIComponent(
      `${v.street}${v.houseNumber ? ' ' + v.houseNumber : ''}, ${v.sector}, ${v.province}, República Dominicana`
    );

    try {
      const res = await fetch(
        `${NOMINATIM_BASE_URL}?q=${query}&format=json&limit=1&countrycodes=do`,
        { headers: { 'Accept-Language': DR_LOCALE } }
      );
      const data = await res.json();
      if (data?.length > 0) {
        this.geocodeLat.set(parseFloat(data[0].lat));
        this.geocodeLon.set(parseFloat(data[0].lon));
      } else {
        this.geocodeError.set('No se encontraron coordenadas. La entrega será sin verificación GPS.');
      }
    } catch {
      this.geocodeError.set('Error al obtener coordenadas.');
    } finally {
      this.geocodeLoading.set(false);
    }
  }

  // ── Helpers ───────────────────────────────────────
  fmtDate(d: Date | string = new Date()): string {
    return new Intl.DateTimeFormat(DR_LOCALE, {
      day: '2-digit', month: 'short', year: 'numeric',
      hour: '2-digit', minute: '2-digit'
    }).format(new Date(d));
  }

  cashierName(): string {
    const u = this.auth.currentUser();
    return u ? `${u.firstName ?? ''} ${u.lastName ?? ''}`.trim() || u.email : 'Cajero';
  }

  logout(): void { this.auth.logout(); }
  toPortal(): void { this.router.navigate(['/portal/dashboard']); }

  totalPages    = computed(() => Math.max(1, Math.ceil(this.filteredCatalog().length / this.PAGE_SIZE)));
  pagedCatalog  = computed(() => {
    const p = this.catalogPage();
    return this.filteredCatalog().slice(p * this.PAGE_SIZE, (p + 1) * this.PAGE_SIZE);
  });

  setSearch(v: string): void { this.catalogSearch.set(v); this.catalogPage.set(0); }
  selectCategory(id: string | null): void { this.selectedCategory.set(id); this.catalogPage.set(0); }
  prevPage(): void { this.catalogPage.update(p => Math.max(0, p - 1)); }
  nextPage(): void { this.catalogPage.update(p => Math.min(this.totalPages() - 1, p + 1)); }

  productInitials(name: string): string {
    return name.split(' ')
      .filter(w => /[a-zA-ZáéíóúÁÉÍÓÚñÑ]/.test(w[0]))
      .slice(0, 2)
      .map(w => w[0])
      .join('')
      .toUpperCase();
  }

  productDisplayLabel(p: CatalogPresentation): string {
    return `${p.productName} — ${p.displayName}`;
  }

  productColor(categoryName: string): string {
    let hash = 0;
    for (let i = 0; i < categoryName.length; i++) hash = categoryName.charCodeAt(i) + ((hash << 5) - hash);
    return PRODUCT_COLORS[Math.abs(hash) % PRODUCT_COLORS.length];
  }
  setPaymentMethod(m: 'cash' | 'card' | 'transfer' | 'credit' | 'delivery'): void { this.paymentMethod.set(m); }
  setCashTendered(v: number): void { this.cashTendered.set(v); }
  setBuyerRnc(v: string): void { this.buyerRnc.set(v); }
}
