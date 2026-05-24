export interface VentaResumen {
  saleId: string;
  receiptNumber: string;
  statusId: number;
  total: number;
  totalPaid: number;
  createdAt: string;
  itemCount: number;
}

export interface VentaItem {
  productId: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  costPrice: number;
  itbisRate: number;
  subtotal: number;
  itbis: number;
  total: number;
}

export interface VentaPago {
  paymentMethodId: number;
  amount: number;
  reference: string | null;
  customerId: string | null;
  receivedAt: string;
}

export interface VentaDetalle {
  saleId: string;
  shiftId: string;
  terminalId: string;
  receiptNumber: string;
  cashierName: string;
  statusId: number;
  subtotal: number;
  totalItbis: number;
  total: number;
  totalPaid: number;
  changeDue: number;
  notes: string | null;
  createdAt: string;
  voidedAt: string | null;
  voidReason: string | null;
  items: VentaItem[];
  payments: VentaPago[];
}

export interface PagedVentas {
  items: VentaResumen[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  totalRevenue: number;
}

export const PAYMENT_METHOD_LABELS: Record<number, string> = {
  1: 'Efectivo',
  2: 'Tarjeta',
  3: 'Transferencia',
  4: 'Crédito',
  5: 'Delivery',
};

export const SALE_STATUS: Record<number, { label: string; variant: string }> = {
  1: { label: 'Completada', variant: 'success' },
  2: { label: 'Anulada', variant: 'error' },
};
