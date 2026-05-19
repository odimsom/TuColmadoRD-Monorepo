import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { GatewayService } from './gateway.service';
import { API_PATHS, PAYMENT_METHOD } from '../constants';

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
  confirmationCode: string;
}

@Injectable({ providedIn: 'root' })
export class DeliveryService {
  private gateway = inject(GatewayService);

  getPendingOrders(): Observable<DeliveryOrderDto[]> {
    return this.gateway.get<DeliveryOrderDto[]>(API_PATHS.DELIVERY_PENDING);
  }

  acceptOrder(id: string): Observable<{ status: string }> {
    return this.gateway.post<{ status: string }>(API_PATHS.DELIVERY_ACCEPT(id), {});
  }

  completeOrder(
    id: string,
    totalAmount: number,
    confirmationCode: string,
    driverLatitude?: number | null,
    driverLongitude?: number | null
  ): Observable<{ status: string }> {
    return this.gateway.post<{ status: string }>(API_PATHS.DELIVERY_COMPLETE(id), {
      payments: [{ paymentMethodId: PAYMENT_METHOD.CASH, amount: totalAmount, reference: null, customerId: null }],
      confirmationCode,
      driverLatitude: driverLatitude ?? null,
      driverLongitude: driverLongitude ?? null
    });
  }
}
