import { Routes } from '@angular/router';

export const authRoutes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./login/login.page').then(m => m.LoginPage),
  },
  {
    path: 'register',
    loadComponent: () => import('./register/register.page').then(m => m.RegisterPage),
  },
  {
    path: 'verify',
    loadComponent: () => import('./verify/verify.page').then(m => m.VerifyPage),
  },
  { path: '', redirectTo: 'login', pathMatch: 'full' },
];
