import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { PagedCompras } from '../models/compra.model';

@Injectable({ providedIn: 'root' })
export class ComprasService {
  private http = inject(HttpClient);
  private api = `${environment.gatewayUrl}/gateway/api/v1/inventory`;

  getCompras(page = 1, pageSize = 20): Observable<PagedCompras> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<PagedCompras>(`${this.api}/stock-entries`, { params });
  }

  createStockEntry(data: {
    purchasedAt: string;
    supplierName: string | null;
    notes: string | null;
    fundId: string | null;
    fundExpenseJustification: string | null;
    lines: Array<{
      presentationId: string;
      containerCount: number;
      unitsPerContainer: number;
      nominalSizePerUnit: number;
      costPerUnit: number;
    }>;
  }): Observable<{ entryId: string }> {
    return this.http.post<{ entryId: string }>(`${this.api}/stock-entries`, data);
  }
}
