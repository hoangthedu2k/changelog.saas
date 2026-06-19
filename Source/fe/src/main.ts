import { bootstrapApplication } from '@angular/platform-browser';
import * as Sentry from '@sentry/angular';
import { appConfig } from './app/app.config';
import { App } from './app/app';
import { environment } from './environments/environment';

if (environment.production && environment.sentryDsn) {
  Sentry.init({
    dsn: environment.sentryDsn,
    environment: 'production',
    tracesSampleRate: 0.2,
    integrations: [Sentry.browserTracingIntegration()],
    beforeSend(event) {
      const msg = event.exception?.values?.[0]?.value ?? '';
      // Ignore errors injected by browser extensions (Zalo, etc.)
      if (/zaloJSV2|chrome-extension|moz-extension/i.test(msg)) return null;
      const frames = event.exception?.values?.[0]?.stacktrace?.frames ?? [];
      if (frames.some(f => /chrome-extension|moz-extension/i.test(f.filename ?? ''))) return null;
      return event;
    },
  });
}

bootstrapApplication(App, appConfig)
  .catch((err) => console.error(err));
