export interface CatalogPresentation {
  id: string;
  productId: string;
  displayName: string;
  presentationType: number;
  presentationTypeName: string;
  sellMode: number;
  sellModeName: string;
  brand: string | null;
  nominalCapacity: number | null;
  measureUnit: number;
  measureUnitName: string;
  salePrice: number;
  costPrice: number;
  isActive: boolean;
}

export interface CatalogItem {
  productId: string;
  name: string;
  categoryId: string;
  categoryName: string;
  itbisRate: number;
  isActive: boolean;
  presentations: CatalogPresentation[];
}

export interface Shift {
  shiftId: string;
  cashierName: string;
  status: string;
  openingCashAmount: number;
  openedAt: string;
  closedAt: string | null;
  totalSalesCount: number;
  totalSalesAmount: number;
}

export interface ShiftSummary {
  shiftId: string;
  openedAt: string;
  initialCash: number;
  totalCashSales: number;
  totalAccountPayments: number;
  totalExpenses: number;
  expectedCash: number;
}

export interface CloseShiftResult {
  shiftId: string;
  totalSalesCount: number;
  totalSalesAmount: number;
  expectedCashAmount: number;
  actualCashAmount: number;
  cashDifference: number;
  closedAt: string;
}

export interface CartLine {
  product: CatalogItem;
  presentation: CatalogPresentation;
  quantity: number;
}

export const PAYMENT_METHODS = { CASH: 1, CREDIT: 2, CARD: 3, TRANSFER: 4 } as const;
export type PaymentMode = 'cash' | 'card' | 'transfer' | 'credit';

export interface CreateSalePayment {
  paymentMethodId: number;
  amount: number;
  reference?: string | null;
  customerId?: string | null;
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
  items: {
    productId: string;
    productName: string;
    quantity: number;
    unitPrice: number;
    lineItbis: number;
    lineTotal: number;
  }[];
}
