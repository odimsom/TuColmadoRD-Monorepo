import { Routes } from '@angular/router';

export const configuracionRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./pages/configuracion.page').then(m => m.ConfiguracionPage),
  },
];
