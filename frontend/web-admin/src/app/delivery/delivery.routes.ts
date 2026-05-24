import { Routes } from '@angular/router';

export const deliveryRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./delivery.page').then(m => m.DeliveryPage),
  },
];
