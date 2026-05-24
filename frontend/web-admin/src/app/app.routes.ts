import { Routes } from '@angular/router';
import { authGuard } from './core/auth.guard';
import { roleGuard } from './core/role.guard';
import { ShellComponent } from './shared/layout/shell/shell.component';

export const routes: Routes = [
  { path: '', redirectTo: '/auth/login', pathMatch: 'full' },

  {
    path: 'auth',
    loadChildren: () => import('./auth/auth.routes').then(m => m.authRoutes),
  },

  {
    path: 'portal',
    component: ShellComponent,
    canActivate: [authGuard, roleGuard('Owner', 'Admin')],
    loadChildren: () => import('./portal/portal.routes').then(m => m.portalRoutes),
  },

  {
    path: 'pos',
    loadChildren: () => import('./pos/pos.routes').then(m => m.posRoutes),
    canActivate: [authGuard, roleGuard('Owner', 'Admin', 'Seller', 'Cashier')],
  },

  {
    path: 'delivery',
    loadChildren: () => import('./delivery/delivery.routes').then(m => m.deliveryRoutes),
    canActivate: [authGuard, roleGuard('Delivery')],
  },

  { path: '**', redirectTo: '/auth/login' },
];
