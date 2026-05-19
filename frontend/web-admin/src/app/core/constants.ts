export const LS_KEYS = {
  TOKEN: 'tc_token',
  USER: 'tc_user',
  TENANT: 'tc_tenant',
  THEME: 'tucolmado-theme',
  DOWNLOAD_BANNER_DISMISSED: 'tc_download_banner_dismissed',
} as const;

export const PAYMENT_METHOD = {
  CASH: 1,
  CARD: 2,
  TRANSFER: 3,
  CREDIT: 4,
  DELIVERY: 5,
} as const;

export const PAYMENT_METHOD_LABELS: Record<number, string> = {
  1: 'Efectivo',
  2: 'Tarjeta',
  3: 'Transferencia',
  4: 'Crédito',
  5: 'Delivery',
} as const;

export const PRESENTATION_TYPE = {
  BULK_CONTAINER: 1,
  PACKAGED_UNIT: 2,
} as const;

export const PRESENTATION_TYPE_LABELS: Record<number, string> = {
  1: 'Contenedor a granel',
  2: 'Unidad empacada',
} as const;

export const SELL_MODE = {
  BY_UNIT: 1,
  BY_WEIGHT: 2,
  BY_CONTAINER: 3,
} as const;

export const SELL_MODE_LABELS: Record<number, string> = {
  1: 'Por unidad',
  2: 'Por peso',
  3: 'Por contenedor',
} as const;

export const CONTAINER_STATUS = {
  SEALED: 1,
  OPEN: 2,
  EMPTY: 3,
} as const;

export const CONTAINER_STATUS_LABELS: Record<number, string> = {
  1: 'Sellado',
  2: 'Abierto',
  3: 'Vacío',
} as const;

export const FUND_TRANSACTION_TYPE = {
  DEPOSIT: 1,
  EXPENSE: 2,
} as const;

export const FUND_TRANSACTION_TYPE_LABELS: Record<number, string> = {
  1: 'Depósito',
  2: 'Gasto',
} as const;

export const EXPENSE_CATEGORY = {
  STOCK_PURCHASE: 1,
  OPERATIONAL: 2,
  LOSS: 3,
  EXTERNAL_FUND: 4,
  OTHER: 5,
} as const;

export const EXPENSE_CATEGORY_LABELS: Record<number, string> = {
  1: 'Compra de inventario',
  2: 'Operacional',
  3: 'Pérdida',
  4: 'Fondo externo',
  5: 'Otro',
} as const;

export const UNIT_OF_MEASURE = {
  POUND:    1,
  OUNCE:    2,
  KILOGRAM: 3,
  UNIT:     10,
  BOX:      11,
  PACKAGE:  12,
  SACK:     13,
  LITER:    20,
  GALLON:   21,
  BOTTLE:   22,
} as const;

export const UNIT_OF_MEASURE_LABELS: Record<number, string> = {
  1:  'Libra',
  2:  'Onza',
  3:  'Kilogramo',
  10: 'Unidad',
  11: 'Caja',
  12: 'Paquete',
  13: 'Saco',
  20: 'Litro',
  21: 'Galón',
  22: 'Botella',
} as const;

export const UNIT_TYPE = {
  UNIT: 1,
  POUND: 2,
  LITER: 3,
  BOX: 4,
} as const;

export const UNIT_TYPE_LABELS: Record<number, string> = {
  1: 'Unidad',
  2: 'Libra',
  3: 'Litro',
  4: 'Caja',
} as const;

export const DEFAULT_ITBIS_RATE = 0.18;
export const DEFAULT_CREDIT_LIMIT = 5000;
export const LOW_STOCK_THRESHOLD = 5;
export const DEFAULT_PAGE_SIZE = 20;
export const CATALOG_PAGE_SIZE = 24;

export const QUICK_CASH_AMOUNTS = [500, 1000, 2000, 5000] as const;

export const PRODUCT_COLORS = [
  'bg-blue-700',
  'bg-violet-700',
  'bg-emerald-700',
  'bg-amber-700',
  'bg-rose-700',
  'bg-cyan-700',
  'bg-indigo-700',
  'bg-teal-700',
] as const;

export const NOMINATIM_BASE_URL = 'https://nominatim.openstreetmap.org/search';
export const OPENFREEMAP_STYLE_URL = 'https://tiles.openfreemap.org/styles/liberty';
export const WHATSAPP_BASE_URL = 'https://wa.me';
export const GOOGLE_MAPS_BASE_URL = 'https://www.google.com/maps/search/?api=1&query=';
export const WAZE_BASE_URL = 'https://waze.com/ul?ll=';
export const DR_LOCALE = 'es-DO';
export const DR_CURRENCY_CODE = 'DOP';
export const SANTO_DOMINGO_COORDS = [-69.9, 18.5] as const;

export const ERROR_MESSAGES: Record<string, string> = {
  'product.sale_price_below_cost': 'El precio de venta no puede ser menor al costo.',
  'product.category_required': 'Selecciona una categoría.',
  'product.name_required': 'El nombre del producto es obligatorio.',
  'product.invalid_price': 'El precio ingresado no es válido.',
  'category.not_found': 'La categoría seleccionada no existe.',
  'auth.token_missing': 'Token de autenticación no encontrado.',
  'fund.insufficient_balance': 'El fondo no tiene balance suficiente.',
  'fund.justification_required': 'Se requiere justificación para gastos mayores al balance.',
} as const;

export const EXPENSE_CATEGORIES_LEGACY = [
  { value: 'Utilities',    label: 'Servicios (Agua/Luz/Tel)' },
  { value: 'Maintenance',  label: 'Mantenimiento' },
  { value: 'Supplies',     label: 'Insumos y suministros' },
  { value: 'Hielo',        label: 'Hielo' },
  { value: 'Personnel',    label: 'Personal' },
  { value: 'Other',        label: 'Otro' },
] as const;

export const ROLE_LABELS: Record<string, string> = {
  Owner: 'Dueño',
  Admin: 'Administrador',
  Seller: 'Vendedor',
  Cashier: 'Cajero',
  Delivery: 'Repartidor',
} as const;

export const SHIFT_STATUS_LABELS: Record<string, string> = {
  Open: 'Abierto',
  Closed: 'Cerrado',
} as const;

export const API_PATHS = {
  INVENTORY_PRODUCTS: '/api/v1/inventory/products',
  INVENTORY_PRODUCTS_SEED: '/api/v1/inventory/products/seed-defaults',
  INVENTORY_CATEGORIES: '/api/v1/inventory/categories',
  INVENTORY_CATALOG: '/api/v1/inventory/catalog',
  INVENTORY_LOW_STOCK: '/api/v1/inventory/products/low-stock',
  INVENTORY_PRESENTATIONS: (productId: string) => `/api/v1/inventory/products/${productId}/presentations`,
  INVENTORY_STOCK_ENTRIES: '/api/v1/inventory/stock-entries',
  INVENTORY_CONTAINER_DRAW: (id: string) => `/api/v1/inventory/containers/${id}/draw`,
  INVENTORY_CONTAINER_EMPTY: (id: string) => `/api/v1/inventory/containers/${id}/empty`,
  INVENTORY_ACTIVE_CONTAINER: (presentationId: string) => `/api/v1/inventory/presentations/${presentationId}/active-container`,
  INVENTORY_OPEN_CONTAINERS: (presentationId: string) => `/api/v1/inventory/presentations/${presentationId}/containers`,
  INVENTORY_FUNDS: '/api/v1/inventory/funds',
  INVENTORY_FUND: (id: string) => `/api/v1/inventory/funds/${id}`,
  INVENTORY_FUND_DEPOSIT: (id: string) => `/api/v1/inventory/funds/${id}/deposit`,
  INVENTORY_FUND_EXPENSE: (id: string) => `/api/v1/inventory/funds/${id}/expense`,
  SALES: '/api/v1/sales',
  SALES_SHIFTS: '/api/v1/sales/shifts',
  SALES_SHIFTS_CURRENT: '/api/v1/sales/shifts/current',
  SALES_SHIFTS_SUMMARY: '/api/v1/sales/shifts/current/summary',
  CUSTOMERS: '/api/v1/customers',
  CUSTOMER_STATEMENT: (id: string) => `/api/v1/customers/${id}/statement`,
  CUSTOMER_PAYMENTS: (id: string) => `/api/v1/customers/${id}/payments`,
  EXPENSES: '/api/v1/expenses',
  DELIVERY_PENDING: '/api/v1/logistics/delivery/pending',
  DELIVERY_ACCEPT: (id: string) => `/api/v1/logistics/delivery/${id}/accept`,
  DELIVERY_COMPLETE: (id: string) => `/api/v1/logistics/delivery/${id}/complete`,
  SETTINGS_PROFILE: '/api/v1/settings/profile',
  REPORTS_SALES: '/api/v1/reports/sales',
  REPORTS_INVENTORY_ALERTS: '/api/v1/reports/inventory-alerts',
  REPORTS_CUSTOMERS: '/api/v1/reports/customers',
  AUTH_LOGIN: '/auth/login',
  AUTH_REGISTER: '/auth/register',
  AUTH_VERIFY_EMAIL: '/auth/verify-email',
  AUTH_EMPLOYEES: '/auth/employees',
} as const;
