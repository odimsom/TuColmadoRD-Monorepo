import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { isDesktopApp } from '../utils/runtime';

export const desktopRegistrationGuard: CanActivateFn = () => {
  if (!isDesktopApp()) {
    return true;
  }

  const router = inject(Router);
  return router.createUrlTree(['/auth/login']);
};