import { Routes } from '@angular/router';

export const cajasRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./pages/lista-cajas.page').then(m => m.ListaCajasPage),
  },
  {
    path: ':id',
    loadComponent: () => import('./pages/detalle-caja.page').then(m => m.DetalleCajaPage),
  },
];
