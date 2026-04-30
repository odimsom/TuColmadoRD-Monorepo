import { Component, signal, inject, OnInit, OnDestroy, ViewChild, ElementRef, AfterViewChecked } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { DeliveryService, DeliveryOrderDto } from '../../core/services/delivery.service';
import { RdCurrencyPipe } from '../../core/pipes';
// maplibre-gl types only — loaded lazily at runtime
// eslint-disable-next-line @typescript-eslint/no-explicit-any
type MaplibreMap = any;
// eslint-disable-next-line @typescript-eslint/no-explicit-any
type MaplibreMarker = any;

@Component({
  selector: 'app-delivery-layout',
  standalone: true,
  imports: [CommonModule, FormsModule, RdCurrencyPipe],
  templateUrl: './delivery-layout.html',
})
export class DeliveryLayout implements OnInit, OnDestroy, AfterViewChecked {
  private auth     = inject(AuthService);
  private delivery = inject(DeliveryService);
  private router   = inject(Router);

  @ViewChild('mapContainer') mapContainerRef?: ElementRef<HTMLDivElement>;

  orders        = signal<DeliveryOrderDto[]>([]);
  loading       = signal(true);
  actingOn      = signal<string | null>(null);
  errorMsg      = signal<string | null>(null);

  // Complete modal state
  showCompleteModal = signal(false);
  completingOrder   = signal<DeliveryOrderDto | null>(null);
  confirmCode       = signal('');
  driverLat         = signal<number | null>(null);
  driverLon         = signal<number | null>(null);
  gpsLoading        = signal(false);
  gpsError          = signal<string | null>(null);
  submittingComplete = signal(false);
  completeError     = signal<string | null>(null);

  private map?: MaplibreMap;
  private destMarker?: MaplibreMarker;
  private driverMarker?: MaplibreMarker;
  private mapInitialized = false;
  private needsMapInit   = false;

  ngOnInit(): void { this.loadOrders(); }

  ngOnDestroy(): void { this.map?.remove(); }

  ngAfterViewChecked(): void {
    if (this.needsMapInit && this.mapContainerRef?.nativeElement) {
      this.needsMapInit = false;
      this.initMap();
    }
  }

  loadOrders(): void {
    this.loading.set(true);
    this.delivery.getPendingOrders().subscribe({
      next: orders => { this.orders.set(orders); this.loading.set(false); },
      error: () => { this.errorMsg.set('Error al cargar pedidos.'); this.loading.set(false); }
    });
  }

  accept(order: DeliveryOrderDto): void {
    this.actingOn.set(order.id);
    this.delivery.acceptOrder(order.id).subscribe({
      next: () => {
        this.orders.update(list => list.map(o =>
          o.id === order.id ? { ...o, status: 'InTransit' } : o
        ));
        this.actingOn.set(null);
      },
      error: () => { this.errorMsg.set('Error al aceptar pedido.'); this.actingOn.set(null); }
    });
  }

  openCompleteModal(order: DeliveryOrderDto): void {
    this.completingOrder.set(order);
    this.confirmCode.set('');
    this.driverLat.set(null);
    this.driverLon.set(null);
    this.gpsError.set(null);
    this.completeError.set(null);
    this.mapInitialized = false;
    this.showCompleteModal.set(true);
    this.needsMapInit = true;
  }

  closeCompleteModal(): void {
    this.showCompleteModal.set(false);
    this.completingOrder.set(null);
    this.map?.remove();
    this.map = undefined;
    this.destMarker = undefined;
    this.driverMarker = undefined;
    this.mapInitialized = false;
  }

  private async initMap(): Promise<void> {
    if (this.mapInitialized || !this.mapContainerRef?.nativeElement) return;
    this.mapInitialized = true;

    const order = this.completingOrder();
    const destLat = order?.latitude;
    const destLon = order?.longitude;

    const { Map, Marker } = await import('maplibre-gl');

    const center: [number, number] = (destLon && destLat)
      ? [destLon, destLat]
      : [-69.9, 18.5]; // Santo Domingo default

    this.map = new Map({
      container: this.mapContainerRef.nativeElement,
      style: 'https://tiles.openfreemap.org/styles/liberty',
      center,
      zoom: destLat ? 15 : 11,
    });

    if (destLat && destLon) {
      const el = document.createElement('div');
      el.className = 'delivery-dest-marker';
      el.style.cssText = 'width:20px;height:20px;background:#ef4444;border-radius:50%;border:3px solid white;box-shadow:0 2px 6px rgba(0,0,0,0.5)';
      this.destMarker = new Marker({ element: el })
        .setLngLat([destLon, destLat])
        .addTo(this.map!);
    }
  }

  captureGps(): void {
    if (!navigator.geolocation) {
      this.gpsError.set('Tu dispositivo no soporta GPS.');
      return;
    }
    this.gpsLoading.set(true);
    this.gpsError.set(null);

    navigator.geolocation.getCurrentPosition(
      pos => {
        const lat = pos.coords.latitude;
        const lon = pos.coords.longitude;
        this.driverLat.set(lat);
        this.driverLon.set(lon);
        this.gpsLoading.set(false);
        this.updateDriverMarker(lat, lon);
      },
      err => {
        this.gpsError.set('No se pudo obtener tu ubicación. Activa el GPS y los permisos.');
        this.gpsLoading.set(false);
      },
      { enableHighAccuracy: true, timeout: 15000 }
    );
  }

  private async updateDriverMarker(lat: number, lon: number): Promise<void> {
    if (!this.map) return;
    const { Marker } = await import('maplibre-gl');
    this.driverMarker?.remove();
    const el = document.createElement('div');
    el.style.cssText = 'width:16px;height:16px;background:#3b82f6;border-radius:50%;border:3px solid white;box-shadow:0 2px 6px rgba(0,0,0,0.5)';
    this.driverMarker = new Marker({ element: el })
      .setLngLat([lon, lat])
      .addTo(this.map);

    const order = this.completingOrder();
    if (order?.latitude && order?.longitude) {
      this.map.fitBounds(
        [[Math.min(lon, order.longitude), Math.min(lat, order.latitude)],
         [Math.max(lon, order.longitude), Math.max(lat, order.latitude)]],
        { padding: 60 }
      );
    } else {
      this.map.flyTo({ center: [lon, lat], zoom: 15 });
    }
  }

  submitComplete(): void {
    const order = this.completingOrder();
    if (!order || this.submittingComplete()) return;

    const code = this.confirmCode().trim();
    if (!code) { this.completeError.set('Ingresa el código de confirmación.'); return; }

    const hasDestCoords = order.latitude && order.longitude;
    if (hasDestCoords && (!this.driverLat() || !this.driverLon())) {
      this.completeError.set('Debes capturar tu ubicación GPS antes de completar.');
      return;
    }

    this.submittingComplete.set(true);
    this.completeError.set(null);

    this.delivery.completeOrder(order.id, order.totalAmount, code, this.driverLat(), this.driverLon()).subscribe({
      next: () => {
        this.orders.update(list => list.filter(o => o.id !== order.id));
        this.closeCompleteModal();
        this.submittingComplete.set(false);
      },
      error: (e) => {
        const msg = e?.error?.detail ?? e?.error?.title ?? 'Error al completar el pedido.';
        this.completeError.set(msg);
        this.submittingComplete.set(false);
      }
    });
  }

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/auth/login']);
  }

  get cashierName(): string {
    return this.auth.currentUser()?.email ?? 'Repartidor';
  }
}
