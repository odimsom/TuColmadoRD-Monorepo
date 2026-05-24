export interface Categoria {
  id: string;
  name: string;
  isActive: boolean;
}

export interface Presentacion {
  id: string;
  displayName: string;
  salePrice: number;
  costPrice: number;
  presentationType: number;
  sellMode: number;
  measureUnit: number;
  brand: string | null;
  nominalCapacity: number | null;
  isActive: boolean;
}

export interface ProductoResumen {
  id: string;
  name: string;
  categoryId: string;
  categoryName: string;
  itbisRate: number;
  isActive: boolean;
}

export interface Producto {
  id: string;
  name: string;
  categoryId: string;
  categoryName: string;
  itbisRate: number;
  isActive: boolean;
  presentations: Presentacion[];
}

export interface PagedProductos {
  items: ProductoResumen[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export const ITBIS_OPTIONS = [
  { label: 'Exento (0%)', value: 0 },
  { label: '16%', value: 0.16 },
  { label: '18%', value: 0.18 },
] as const;

export const SELL_MODE_LABELS: Record<number, string> = {
  1: 'Por unidad',
  2: 'Por peso',
  3: 'Por contenedor',
};

export const SELL_MODE_OPTIONS = [
  { label: 'Por unidad', value: 1 },
  { label: 'Por peso', value: 2 },
  { label: 'Por contenedor', value: 3 },
] as const;

export const PRESENTATION_TYPE_LABELS: Record<number, string> = {
  1: 'A granel',
  2: 'Empacado',
};

export const PRESENTATION_TYPE_OPTIONS = [
  { label: 'A granel', value: 1 },
  { label: 'Empacado', value: 2 },
] as const;

export const MEASURE_UNIT_LABELS: Record<number, string> = {
  1: 'Unidad',
  2: 'Kilogramo',
  3: 'Litro',
  4: 'Gramo',
  5: 'Libra',
};

export const MEASURE_UNIT_OPTIONS = [
  { label: 'Unidad', value: 1 },
  { label: 'Kilogramo', value: 2 },
  { label: 'Litro', value: 3 },
  { label: 'Gramo', value: 4 },
  { label: 'Libra', value: 5 },
] as const;
