import { Routes } from '@angular/router';

export const deliveriesRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./pages/lista-deliveries.page').then(m => m.ListaDeliveriesPage),
  },
];
