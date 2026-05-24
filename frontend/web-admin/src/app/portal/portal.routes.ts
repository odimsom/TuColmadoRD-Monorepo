import { Routes } from '@angular/router';
import { businessSetupGuard } from '../core/business-setup.guard';

export const portalRoutes: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  {
    path: '',
    canActivateChild: [businessSetupGuard],
    children: [
      {
        path: 'dashboard',
        loadComponent: () => import('./dashboard/pages/dashboard.page').then(m => m.DashboardPage),
      },
      {
        path: 'inventario',
        loadChildren: () => import('./inventario/inventario.routes').then(m => m.inventarioRoutes),
      },
      {
        path: 'ventas',
        loadChildren: () => import('./ventas/ventas.routes').then(m => m.ventasRoutes),
      },
      {
        path: 'compras',
        loadChildren: () => import('./compras/compras.routes').then(m => m.comprasRoutes),
      },
      {
        path: 'cajas',
        loadChildren: () => import('./cajas/cajas.routes').then(m => m.cajasRoutes),
      },
      {
        path: 'clientes',
        loadChildren: () => import('./clientes/clientes.routes').then(m => m.clientesRoutes),
      },
      {
        path: 'empleados',
        loadChildren: () => import('./empleados/empleados.routes').then(m => m.empleadosRoutes),
      },
      {
        path: 'deliveries',
        loadChildren: () => import('./deliveries/deliveries.routes').then(m => m.deliveriesRoutes),
      },
      {
        path: 'reportes',
        loadChildren: () => import('./reportes/reportes.routes').then(m => m.reportesRoutes),
      },
      {
        path: 'configuracion',
        loadChildren: () => import('./configuracion/configuracion.routes').then(m => m.configuracionRoutes),
      },
    ],
  },
];
