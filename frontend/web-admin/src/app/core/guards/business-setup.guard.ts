import { inject } from '@angular/core';
import { Router, CanActivateChildFn } from '@angular/router';
import { SettingsService } from '../services/settings.service';
import { firstValueFrom } from 'rxjs';

export const businessSetupGuard: CanActivateChildFn = async (_childRoute, state) => {
  const settings = inject(SettingsService);
  const router = inject(Router);

  if (state.url.startsWith('/portal/settings')) {
    return true;
  }

  try {
    const profile = await firstValueFrom(settings.getProfile());
    if (!profile) {
      return router.createUrlTree(['/portal/settings']);
    }
    return true;
  } catch {
    return true;
  }
};
