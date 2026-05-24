export interface Empleado {
  id: string;
  email: string;
  firstName: string | null;
  lastName: string | null;
  role: string;
  isActive: boolean;
}

export const ROLE_LABELS: Record<string, string> = {
  Owner: 'Dueño',
  Admin: 'Administrador',
  Seller: 'Vendedor',
  Cashier: 'Cajero',
  Delivery: 'Repartidor',
};

export const ROLE_VARIANTS: Record<string, string> = {
  Owner: 'primary',
  Admin: 'secondary',
  Seller: 'info',
  Cashier: 'accent',
  Delivery: 'neutral',
};
