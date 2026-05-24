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
}
