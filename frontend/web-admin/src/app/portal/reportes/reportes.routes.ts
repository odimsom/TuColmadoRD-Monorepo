import { Routes } from '@angular/router';

export const reportesRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./pages/lista-reportes.page').then(m => m.ListaReportesPage),
  },
];
