import { Component, signal, inject, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DeliveryService, DeliveryOrderDto } from '../../../core/services/delivery.service';
import { RdCurrencyPipe } from '../../../core/pipes';

@Component({
  selector: 'app-deliveries',
  standalone: true,
  imports: [CommonModule, RdCurrencyPipe],
  templateUrl: './deliveries.html',
})
export class Deliveries implements OnInit {
  private delivery = inject(DeliveryService);

  orders  = signal<DeliveryOrderDto[]>([]);
  loading = signal(true);
  error   = signal<string | null>(null);

  pending   = computed(() => this.orders().filter(o => o.status === 'Pending'));
  inTransit = computed(() => this.orders().filter(o => o.status === 'InTransit'));

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.delivery.getPendingOrders().subscribe({
      next:  o  => { this.orders.set(o); this.loading.set(false); },
      error: () => { this.error.set('Error al cargar pedidos.'); this.loading.set(false); }
    });
  }

  mapsUrl(o: DeliveryOrderDto): string {
    if (o.latitude && o.longitude)
      return `https://www.google.com/maps?q=${o.latitude},${o.longitude}`;
    return `https://www.google.com/maps/search/${encodeURIComponent(`${o.street}, ${o.sector}, ${o.province}, República Dominicana`)}`;
  }
}
