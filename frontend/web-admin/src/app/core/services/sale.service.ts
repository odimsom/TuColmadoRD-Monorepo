import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { GatewayService } from './gateway.service';

export interface SaleSummary {
  id: string;
  receiptNumber: string;
  statusId: number;
  totalAmount: number;
  totalPaidAmount: number;
  createdAt: string;
  itemsCount: number;
}

export interface PagedSalesResponse {
  items: SaleSummary[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

@Injectable({
  providedIn: 'root'
})
export class SaleService {
  private gateway = inject(GatewayService);

  getSales(page = 1, pageSize = 10): Observable<PagedSalesResponse> {
    return this.gateway.get<PagedSalesResponse>('/sales', { page, pageSize });
  }

  createSale(cmd: any): Observable<any> {
    return this.gateway.post('/sales', cmd);
  }

  getCurrentShift(): Observable<any> {
    return this.gateway.get('/shifts/current');
  }
}
