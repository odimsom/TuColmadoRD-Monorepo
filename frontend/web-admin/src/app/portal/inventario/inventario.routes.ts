import { Routes } from '@angular/router';

export const inventarioRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./pages/lista-inventario.page').then(m => m.ListaInventarioPage),
  },
  {
    path: ':id',
    loadComponent: () => import('./pages/detalle-producto.page').then(m => m.DetalleProductoPage),
  },
];
