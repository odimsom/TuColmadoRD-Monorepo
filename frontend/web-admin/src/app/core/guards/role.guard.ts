import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

/**
 * Factory para crear un guard de rol.
 *
 * Uso en rutas:
 *   canActivate: [roleGuard('Owner', 'Admin')]
 *
 * Roles disponibles (según JWT / AuthUser.role):
 *   Owner | Admin | Cashier
 */
export function roleGuard(...allowedRoles: string[]): CanActivateFn {
  return () => {
    const authService = inject(AuthService);
    const router = inject(Router);

    if (!authService.isAuthenticated()) {
      router.navigate(['/auth/login']);
      return false;
    }

    const user = authService.currentUser();
    if (!user) {
      router.navigate(['/auth/login']);
      return false;
    }

    const userRole = user.role ?? '';
    if (allowedRoles.map(r => r.toLowerCase()).includes(userRole.toLowerCase())) {
      return true;
    }

    if (userRole.toLowerCase() === 'delivery') {
      router.navigate(['/delivery']);
    } else if (['seller', 'cashier'].includes(userRole.toLowerCase())) {
      router.navigate(['/pos']);
    } else {
      router.navigate(['/auth/login']);
    }

    return false;
  };
}
