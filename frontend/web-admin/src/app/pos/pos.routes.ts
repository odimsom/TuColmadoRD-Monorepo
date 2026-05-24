import { Routes } from '@angular/router';

export const posRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./pos.page').then(m => m.PosPage),
  },
];
