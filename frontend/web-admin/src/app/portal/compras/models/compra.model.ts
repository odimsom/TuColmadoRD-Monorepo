export interface CompraLinea {
  presentationId: string;
  presentationName: string;
  containerCount: number;
  unitsPerContainer: number;
  nominalSizePerUnit: number;
  costPerUnit: number;
}

export interface CompraResumen {
  id: string;
  purchasedAt: string;
  supplierName: string | null;
  notes: string | null;
  totalCost: number;
  lineCount: number;
}

export interface PagedCompras {
  items: CompraResumen[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}
