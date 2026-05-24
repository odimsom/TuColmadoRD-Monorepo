import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { PagedVentas, VentaDetalle } from '../models/venta.model';

@Injectable({ providedIn: 'root' })
export class VentasService {
  private http = inject(HttpClient);
  private api = `${environment.gatewayUrl}/gateway/api/v1/sales`;

  getVentas(page = 1, pageSize = 20): Observable<PagedVentas> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<PagedVentas>(this.api, { params });
  }

  getVenta(id: string): Observable<VentaDetalle> {
    return this.http.get<VentaDetalle>(`${this.api}/${id}`);
  }

  anularVenta(id: string, voidReason: string): Observable<void> {
    return this.http.post<void>(`${this.api}/${id}/void`, { voidReason });
  }
}
