import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { provideClientHydration, withEventReplay } from '@angular/platform-browser';
import { authGuard } from './core/auth/auth.guard';
import { planGuard } from './core/auth/plan.guard';
import {authInterceptor} from "./core/auth/auth.interceptor";
import { provideHttpClient, withInterceptors } from '@angular/common/http';
export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes), provideClientHydration(withEventReplay()),
    provideHttpClient(withInterceptors([authInterceptor])),
  ]
};
