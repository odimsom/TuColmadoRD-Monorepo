import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

export function roleGuard(...roles: string[]): CanActivateFn {
  return () => {
    const auth = inject(AuthService);
    const router = inject(Router);

    if (!auth.isAuthenticated()) return router.createUrlTree(['/auth/login']);

    const user = auth.currentUser();
    if (!user) return router.createUrlTree(['/auth/login']);

    const role = (user.role ?? '').toLowerCase();
    if (roles.map(r => r.toLowerCase()).includes(role)) return true;

    if (role === 'delivery') return router.createUrlTree(['/delivery']);
    if (['seller', 'cashier'].includes(role)) return router.createUrlTree(['/pos']);
    return router.createUrlTree(['/auth/login']);
  };
}
