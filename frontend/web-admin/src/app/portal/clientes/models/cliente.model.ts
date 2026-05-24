export interface Cliente {
  customerId: string;
  fullName: string;
  phone: string | null;
  balance: number;
  creditLimit: number;
  isActive: boolean;
  province?: string | null;
  sector?: string | null;
  street?: string | null;
}

export interface ClienteEstadoCuenta {
  transactionId: string;
  date: string;
  type: string;
  amount: number;
  concept: string;
}

export interface PagedClientes {
  items: Cliente[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}
