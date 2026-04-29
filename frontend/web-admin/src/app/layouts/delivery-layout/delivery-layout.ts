import { Component, signal, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { DeliveryService, DeliveryOrderDto } from '../../core/services/delivery.service';
import { RdCurrencyPipe } from '../../core/pipes';

@Component({
  selector: 'app-delivery-layout',
  standalone: true,
  imports: [CommonModule, RdCurrencyPipe],
  templateUrl: './delivery-layout.html',
})
export class DeliveryLayout implements OnInit {
  private auth     = inject(AuthService);
  private delivery = inject(DeliveryService);
  private router   = inject(Router);

  orders    = signal<DeliveryOrderDto[]>([]);
  loading   = signal(true);
  actingOn  = signal<string | null>(null);
  errorMsg  = signal<string | null>(null);

  ngOnInit(): void {
    this.loadOrders();
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

  complete(order: DeliveryOrderDto): void {
    this.actingOn.set(order.id);
    this.delivery.completeOrder(order.id, order.totalAmount).subscribe({
      next: () => {
        this.orders.update(list => list.filter(o => o.id !== order.id));
        this.actingOn.set(null);
      },
      error: () => { this.errorMsg.set('Error al completar pedido.'); this.actingOn.set(null); }
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
