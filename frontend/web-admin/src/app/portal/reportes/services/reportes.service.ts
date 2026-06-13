import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { ReporteVentas, ReporteInventario, ReporteClientes } from '../models/reporte.model';

@Injectable({ providedIn: 'root' })
export class ReportesService {
  private http = inject(HttpClient);
  // El reports-service (Rust) responde en snake_case y exige tenant_id en query
  private api = `${environment.gatewayUrl}/gateway/api/v1/reports`;

  private params(from?: string, to?: string): HttpParams {
    let p = new HttpParams().set('tenant_id', localStorage.getItem('tc_tenant') ?? '');
    if (from) p = p.set('from', from);
    if (to) p = p.set('to', to);
    return p;
  }

  getVentas(from?: string, to?: string): Observable<ReporteVentas> {
    return this.http.get<ReporteVentas>(`${this.api}/sales`, { params: this.params(from, to) });
  }

  getInventario(): Observable<ReporteInventario> {
    return this.http.get<ReporteInventario>(`${this.api}/inventory-alerts`, { params: this.params() });
  }

  getClientes(): Observable<ReporteClientes> {
    return this.http.get<ReporteClientes>(`${this.api}/customers`, { params: this.params() });
  }
}
