import { ApplicationConfig, ErrorHandler, provideBrowserGlobalErrorListeners, LOCALE_ID } from '@angular/core';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { registerLocaleData } from '@angular/common';
import localeEsDo from '@angular/common/locales/es-DO';
import 'iconify-icon';

registerLocaleData(localeEsDo);

import { routes } from './app.routes';
import { authInterceptor } from './core/http.interceptor';
import { AppErrorHandler } from './core/error.handler';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes, withComponentInputBinding()),
    provideHttpClient(withInterceptors([authInterceptor])),
    { provide: ErrorHandler, useClass: AppErrorHandler },
    { provide: LOCALE_ID, useValue: 'es-DO' },
  ],
};
