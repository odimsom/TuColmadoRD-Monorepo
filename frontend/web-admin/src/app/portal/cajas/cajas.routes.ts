import { Routes } from '@angular/router';

export const cajasRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./pages/lista-cajas.page').then(m => m.ListaCajasPage),
  },
];
