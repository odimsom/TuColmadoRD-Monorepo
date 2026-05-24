import { inject } from '@angular/core';
import { CanActivateChildFn, Router } from '@angular/router';
import { SettingsService } from './settings.service';
import { firstValueFrom } from 'rxjs';

export const businessSetupGuard: CanActivateChildFn = async (_child, state) => {
  if (state.url.startsWith('/portal/configuracion')) return true;

  const settings = inject(SettingsService);
  const router = inject(Router);

  try {
    const profile = await firstValueFrom(settings.getProfile());
    return profile ? true : router.createUrlTree(['/portal/configuracion']);
  } catch {
    return true;
  }
};
