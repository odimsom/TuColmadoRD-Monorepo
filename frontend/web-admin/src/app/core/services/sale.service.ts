import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { GatewayService } from './gateway.service';
import { API_PATHS } from '../constants';

export interface SaleSummary {
  saleId: string;
  receiptNumber: string;
  statusId: number;
  total: number;
  totalPaid: number;
  createdAt: string;
  itemCount: number;
}

export interface PagedSalesResponse {
  items: SaleSummary[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  totalRevenue: number;
}

export interface CreateSaleItemRequest {
  productId: string;
  presentationId: string;
  quantity: number;
}

export interface CreateSalePaymentRequest {
  paymentMethodId: number;
  amount: number;
  reference?: string;
  customerId?: string;
}

export interface DeliveryAddressRequest {
  province: string;
  sector: string;
  street: string;
  reference: string;
  houseNumber?: string;
  latitude?: number;
  longitude?: number;
}

export interface CreateSaleRequest {
  items: CreateSaleItemRequest[];
  payments: CreateSalePaymentRequest[];
  notes?: string | null;
  buyerRnc?: string | null;
  deliveryAddress?: DeliveryAddressRequest | null;
}

export interface CreateSaleLineResult {
  productId: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  lineItbis: number;
  lineTotal: number;
}

export interface CreateSaleResult {
  saleId: string;
  receiptNumber: string;
  ncfNumber: string | null;
  subtotal: number;
  totalItbis: number;
  total: number;
  totalPaid: number;
  changeDue: number;
  items: CreateSaleLineResult[];
  deliveryOrderId?: string | null;
  confirmationCode?: string | null;
}

export interface ShiftDto {
  shiftId: string;
  tenantId: string;
  terminalId: string;
  cashierName: string;
  status: string;
  openingCashAmount: number;
  closingCashAmount: number | null;
  openedAt: string;
  closedAt: string | null;
  expectedCashAmount: number | null;
  actualCashAmount: number | null;
  cashDifference: number | null;
  notes: string | null;
  totalSalesCount: number;
  totalSalesAmount: number;
}

export interface ShiftSummaryDto {
  shiftId: string;
  openedAt: string;
  openingCashAmount: number;
  totalCashSales: number;
  totalAccountPayments: number;
  totalExpenses: number;
  expectedCashAmount: number;
}

export interface OpenShiftRequest {
  openingCashAmount: number;
  cashierName: string;
}

export interface CloseShiftRequest {
  actualCashAmount: number;
  notes?: string;
}

@Injectable({
  providedIn: 'root'
})
export class SaleService {
  private gateway = inject(GatewayService);

  getSales(page = 1, pageSize = 10): Observable<PagedSalesResponse> {
    return this.gateway.get<PagedSalesResponse>(API_PATHS.SALES, { page, pageSize });
  }

  createSale(cmd: CreateSaleRequest): Observable<CreateSaleResult> {
    return this.gateway.post<CreateSaleResult>(API_PATHS.SALES, cmd);
  }

  voidSale(id: string, reason: string): Observable<void> {
    return this.gateway.post(`${API_PATHS.SALES}/${id}/void`, { voidReason: reason });
  }

  getCurrentShift(): Observable<ShiftDto> {
    return this.gateway.get<ShiftDto>(API_PATHS.SALES_SHIFTS_CURRENT);
  }

  openShift(cmd: OpenShiftRequest): Observable<{ shiftId: string }> {
    return this.gateway.post(API_PATHS.SALES_SHIFTS + '/open', cmd);
  }

  closeShift(shiftId: string, cmd: CloseShiftRequest): Observable<any> {
    return this.gateway.post(`${API_PATHS.SALES_SHIFTS}/${shiftId}/close`, cmd);
  }

  getShiftsPaged(page = 1, pageSize = 20, status = 'all'): Observable<any> {
    return this.gateway.get(API_PATHS.SALES_SHIFTS, { page, pageSize, status });
  }

  getShiftSummary(): Observable<ShiftSummaryDto> {
    return this.gateway.get<ShiftSummaryDto>(API_PATHS.SALES_SHIFTS_SUMMARY);
  }
}
