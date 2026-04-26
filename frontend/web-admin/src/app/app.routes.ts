import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { desktopRegistrationGuard } from './core/guards/desktop-registration.guard';
import { roleGuard } from './core/guards/role.guard';

export const routes: Routes = [
  // Ruta pública — bloqueada en desktop (redirige a login)
  {
    path: '',
    canActivate: [desktopRegistrationGuard],
    loadComponent: () => import('./layouts/public-layout/public-layout').then(m => m.PublicLayout),
    children: [
      {
        path: '',
        loadComponent: () => import('./features/public/home/home').then(m => m.Home)
      }
    ]
  },

  // Auth
  {
    path: 'auth',
    loadComponent: () => import('./layouts/auth-layout/auth-layout').then(m => m.AuthLayout),
    children: [
      {
        path: 'login',
        loadComponent: () => import('./features/auth/login/login').then(m => m.Login)
      },
      {
        // Registro bloqueado en desktop (ya hay licencia activa)
        path: 'register',
        canActivate: [desktopRegistrationGuard],
        loadComponent: () => import('./features/auth/register/register').then(m => m.Register)
      }
    ]
  },

  // Portal Admin — requiere autenticación + rol Owner o Admin
  {
    path: 'portal',
    loadComponent: () => import('./layouts/portal-layout/portal-layout').then(m => m.PortalLayout),
    canActivate: [authGuard, roleGuard('Owner', 'Admin')],
    children: [
      {
        path: 'welcome',
        loadComponent: () => import('./features/portal/welcome/welcome').then(m => m.Welcome)
      },
      {
        path: 'dashboard',
        loadComponent: () => import('./features/portal/dashboard/dashboard').then(m => m.Dashboard)
      },
      {
        path: 'inventory',
        loadComponent: () => import('./features/portal/inventory/inventory').then(m => m.Inventory)
      },
      {
        path: 'sales',
        loadComponent: () => import('./features/portal/sales/sales').then(m => m.Sales)
      },
      {
        path: 'customers',
        loadComponent: () => import('./features/portal/customers/customers').then(m => m.Customers)
      },
      {
        path: 'subscription',
        loadComponent: () => import('./features/portal/subscription/subscription').then(m => m.Subscription)
      },
      {
        path: 'settings',
        loadComponent: () => import('./features/portal/settings/settings').then(m => m.Settings)
      },
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
      }
    ]
  },

  // POS — requiere autenticación (cualquier rol)
  {
    path: 'pos',
    loadComponent: () => import('./layouts/pos-layout/pos-layout').then(m => m.PosLayout),
    canActivate: [authGuard]
  }
];
