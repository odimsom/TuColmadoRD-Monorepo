import { Routes } from '@angular/router';

export const clientesRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./pages/lista-clientes.page').then(m => m.ListaClientesPage),
  },
  {
    path: ':id',
    loadComponent: () => import('./pages/detalle-cliente.page').then(m => m.DetalleClientePage),
  },
];
