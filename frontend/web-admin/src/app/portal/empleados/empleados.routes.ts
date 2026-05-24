import { Routes } from '@angular/router';

export const empleadosRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./pages/lista-empleados.page').then(m => m.ListaEmpleadosPage),
  },
];
