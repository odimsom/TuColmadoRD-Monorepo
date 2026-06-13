import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { DeliveryPendiente } from '../models/delivery.model';

@Injectable({ providedIn: 'root' })
export class DeliveriesService {
  private http = inject(HttpClient);
  private api = `${environment.gatewayUrl}/gateway/api/v1/logistics/delivery`;

  getPendientes(): Observable<DeliveryPendiente[]> {
    return this.http.get<DeliveryPendiente[]>(`${this.api}/pending`);
  }

  aceptar(id: string): Observable<{ status: string }> {
    return this.http.post<{ status: string }>(`${this.api}/${id}/accept`, {});
  }

  completar(
    id: string,
    payments: { paymentMethodId: number; amount: number; reference?: string | null; customerId?: string | null }[],
    confirmationCode: string,
  ): Observable<{ status: string }> {
    return this.http.post<{ status: string }>(`${this.api}/${id}/complete`, {
      payments, confirmationCode, driverLatitude: null, driverLongitude: null,
    });
  }
}
