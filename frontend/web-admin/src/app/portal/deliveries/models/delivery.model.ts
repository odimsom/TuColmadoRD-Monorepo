// Coincide con DeliveryOrderDto del Core API
export interface DeliveryPendiente {
  id: string;
  saleId: string;
  receiptNumber: string;
  totalAmount: number;
  customerName: string;
  phone: string;
  addressProvince: string;
  addressSector: string;
  addressStreet: string;
  addressHouseNumber: string | null;
  addressReference: string;
  status: string;
  createdAt: string;
}

export function direccionCompleta(d: DeliveryPendiente): string {
  return [d.addressStreet, d.addressHouseNumber, d.addressSector, d.addressProvince]
    .filter(Boolean)
    .join(', ');
}
