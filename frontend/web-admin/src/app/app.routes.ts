import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { desktopRegistrationGuard } from './core/guards/desktop-registration.guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./layouts/public-layout/public-layout').then(m => m.PublicLayout),
    children: [
      {
        path: '',
        loadComponent: () => import('./features/public/home/home').then(m => m.Home)
      }
    ]
  },
  {
    path: 'auth',
    loadComponent: () => import('./layouts/auth-layout/auth-layout').then(m => m.AuthLayout),
    children: [
      {
        path: 'login',
        loadComponent: () => import('./features/auth/login/login').then(m => m.Login)
      },
      {
        path: 'register',
        canActivate: [desktopRegistrationGuard],
        loadComponent: () => import('./features/auth/register/register').then(m => m.Register)
      }
    ]
  },
  {
    path: 'portal',
    loadComponent: () => import('./layouts/portal-layout/portal-layout').then(m => m.PortalLayout),
    canActivate: [authGuard],
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
        path: 'subscription',
        loadComponent: () => import('./features/portal/subscription/subscription').then(m => m.Subscription)
      },
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
      }
    ]
  },
  {
    path: 'pos',
    loadComponent: () => import('./layouts/pos-layout/pos-layout').then(m => m.PosLayout),
    canActivate: [authGuard]
  }
];
