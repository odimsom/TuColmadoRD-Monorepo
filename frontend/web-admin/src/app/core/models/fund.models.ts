export interface MonetaryFundDto {
  id: string;
  tenantId: string;
  name: string;
  currentBalance: number;
  createdAt: string;
}

export interface FundTransactionDto {
  id: string;
  fundId: string;
  type: number;
  typeName: string;
  amount: number;
  category: number | null;
  categoryName: string | null;
  description: string;
  justificationNote: string | null;
  referenceId: string | null;
  balanceAfter: number;
  occurredAt: string;
}

export interface CreateMonetaryFundRequest {
  name: string;
  initialDeposit: number;
}

export interface RecordFundDepositRequest {
  amount: number;
  description: string;
}

export interface RecordFundExpenseRequest {
  amount: number;
  category: number;
  description: string;
  justificationNote?: string | null;
  referenceId?: string | null;
}

export interface FundBalanceResponse {
  fund: MonetaryFundDto;
  recentTransactions: FundTransactionDto[];
}

export interface PagedFundTransactionsResponse {
  items: FundTransactionDto[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}
