export interface DeliveryPendiente {
  orderId: string;
  saleId: string;
  receiptNumber: string;
  customerName: string | null;
  address: string;
  createdAt: string;
  status: string;
}
