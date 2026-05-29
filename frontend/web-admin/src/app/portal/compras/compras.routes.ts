import { Routes } from '@angular/router';

export const comprasRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./pages/lista-compras.page').then(m => m.ListaComprasPage),
  },
  {
    path: 'nueva',
    loadComponent: () => import('./pages/nueva-compra.page').then(m => m.NuevaCompraPage),
  },
];
