import { Routes } from '@angular/router';

export const ventasRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./pages/lista-ventas.page').then(m => m.ListaVentasPage),
  },
];
