import { Routes } from '@angular/router';

export const clientesRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./pages/lista-clientes.page').then(m => m.ListaClientesPage),
  },
];
