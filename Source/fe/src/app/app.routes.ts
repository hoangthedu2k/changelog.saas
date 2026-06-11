import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';
import { guestGuard } from './core/auth/guest.guard';

export const routes: Routes = [
    { path: 'login', canActivate: [guestGuard], loadComponent: () => import('./features/auth/login/login').then(m => m.Login) },
    { path: 'register', canActivate: [guestGuard], loadComponent: () => import('./features/auth/register/register').then(m => m.Register) },
    {
        path: 'app',
        loadComponent: () => import('./shared/layout/app-shell/app-shell').then(m => m.AppShell),
        canActivate: [authGuard],
        children: [
            { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
            { path: 'dashboard', loadComponent: () => import('./features/dashboard/dashboard/dashboard').then(m => m.Dashboard) },
            { path: 'entries', loadComponent: () => import('./features/entries/entry-list/entry-list').then(m => m.EntryList) },
            { path: 'entries/new', loadComponent: () => import('./features/entries/entry-editor/entry-editor').then(m => m.EntryEditor) },
            { path: 'entries/:id/edit', loadComponent: () => import('./features/entries/entry-editor/entry-editor').then(m => m.EntryEditor) },
            { path: 'subscribers', loadComponent: () => import('./features/subscribers/subscriber-list/subscriber-list').then(m => m.SubscriberList) },
            { path: 'widget', loadComponent: () => import('./features/widget/widget-setup/widget-setup').then(m => m.WidgetSetup) },
            { path: 'settings', redirectTo: 'settings/appearance', pathMatch: 'full' },
            { path: 'settings/appearance', loadComponent: () => import('./features/settings/appearance/appearance').then(m => m.Appearance) },
            { path: 'settings/billing', loadComponent: () => import('./features/settings/billing/billing').then(m => m.Billing) },
            { path: 'settings/project', loadComponent: () => import('./features/settings/project/project-settings').then(m => m.ProjectSettings) },
        ],
    },
    { path: 'c/:slug', loadComponent: () => import('./public/changelog-page/changelog-page').then(m => m.ChangelogPage) },
    { path: '', redirectTo: '/app', pathMatch: 'full' },
    { path: '**', redirectTo: '/app' },
];
