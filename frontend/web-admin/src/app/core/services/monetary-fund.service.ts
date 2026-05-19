import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { GatewayService } from './gateway.service';
import {
  MonetaryFundDto,
  FundTransactionDto,
  CreateMonetaryFundRequest,
  RecordFundDepositRequest,
  RecordFundExpenseRequest,
  FundBalanceResponse,
  PagedFundTransactionsResponse,
} from '../models/fund.models';
export type { MonetaryFundDto, FundTransactionDto, FundBalanceResponse } from '../models/fund.models';
import { API_PATHS } from '../constants';

@Injectable({ providedIn: 'root' })
export class MonetaryFundService {
  private gateway = inject(GatewayService);

  getFunds(): Observable<MonetaryFundDto[]> {
    return this.gateway.get<MonetaryFundDto[]>(API_PATHS.INVENTORY_FUNDS);
  }

  getFund(id: string): Observable<FundBalanceResponse> {
    return this.gateway.get<FundBalanceResponse>(API_PATHS.INVENTORY_FUND(id));
  }

  createFund(cmd: CreateMonetaryFundRequest): Observable<{ id: string }> {
    return this.gateway.post(API_PATHS.INVENTORY_FUNDS, cmd);
  }

  recordDeposit(fundId: string, cmd: RecordFundDepositRequest): Observable<{ transactionId: string }> {
    return this.gateway.post(API_PATHS.INVENTORY_FUND_DEPOSIT(fundId), cmd);
  }

  recordExpense(fundId: string, cmd: RecordFundExpenseRequest): Observable<{ transactionId: string }> {
    return this.gateway.post(API_PATHS.INVENTORY_FUND_EXPENSE(fundId), cmd);
  }

  getTransactions(fundId: string, page = 1, pageSize = 20): Observable<PagedFundTransactionsResponse> {
    return this.gateway.get<PagedFundTransactionsResponse>(`${API_PATHS.INVENTORY_FUND(fundId)}/transactions`, { page, pageSize });
  }
}
