import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { GatewayService } from './gateway.service';

export interface DeliveryOrderDto {
  id: string;
  saleId: string;
  receiptNumber: string;
  totalAmount: number;
  customerName: string;
  customerPhone: string;
  province: string;
  sector: string;
  street: string;
  houseNumber?: string | null;
  reference: string;
  latitude?: number | null;
  longitude?: number | null;
  status: string;
}

@Injectable({ providedIn: 'root' })
export class DeliveryService {
  private gateway = inject(GatewayService);

  getPendingOrders(): Observable<DeliveryOrderDto[]> {
    return this.gateway.get<DeliveryOrderDto[]>('/api/v1/logistics/delivery/pending');
  }

  acceptOrder(id: string): Observable<{ status: string }> {
    return this.gateway.post<{ status: string }>(`/api/v1/logistics/delivery/${id}/accept`, {});
  }

  completeOrder(id: string, totalAmount: number): Observable<{ status: string }> {
    return this.gateway.post<{ status: string }>(`/api/v1/logistics/delivery/${id}/complete`, [
      { paymentMethodId: 1, amount: totalAmount, reference: null, customerId: null }
    ]);
  }
}
