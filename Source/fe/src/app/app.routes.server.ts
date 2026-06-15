import { RenderMode, ServerRoute } from '@angular/ssr';

export const serverRoutes: ServerRoute[] = [
  // Parameterized routes must be SSR (on-demand) — cannot prerender without known params
  { path: 'app/entries/:id/edit', renderMode: RenderMode.Server },
  { path: 'c/:slug', renderMode: RenderMode.Server },
  // Everything else: prerender at build time
  { path: '**', renderMode: RenderMode.Prerender },
];
