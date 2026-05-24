export interface FondoMonetario {
  id: string;
  name: string;
  balance: number;
  isActive: boolean;
  createdAt: string;
}

export interface FondoTransaccion {
  id: string;
  type: number;
  amount: number;
  description: string;
  category: string | null;
  createdAt: string;
}

export interface PagedTransacciones {
  items: FondoTransaccion[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export const TRANSACTION_TYPE_LABELS: Record<number, string> = {
  1: 'Depósito',
  2: 'Gasto',
};

export const TRANSACTION_TYPE_VARIANTS: Record<number, string> = {
  1: 'success',
  2: 'error',
};

export const EXPENSE_CATEGORIES = [
  { value: 'Utilities', label: 'Servicios (Agua/Luz/Tel)' },
  { value: 'Maintenance', label: 'Mantenimiento' },
  { value: 'Supplies', label: 'Insumos y suministros' },
  { value: 'Hielo', label: 'Hielo' },
  { value: 'Personnel', label: 'Personal' },
  { value: 'Other', label: 'Otro' },
] as const;
