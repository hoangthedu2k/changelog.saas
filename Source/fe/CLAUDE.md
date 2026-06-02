# ChangelogSaaS — Frontend CLAUDE.md

Frontend context. Đọc trước khi làm bất cứ thứ gì trong `Source/fe/`.

## Build & Run

```bash
cd changelog-app

# Install dependencies
npm install

# Dev server (http://localhost:4200)
ng serve

# Build production (SSR)
ng build

# Run tests (vitest)
npm test

# Generate component
ng g c features/entries/entry-list --standalone
```

## Tech Stack
- Angular **21** (standalone, Signals, SSR/Hydration)
- TypeScript 5.9
- Vitest (không phải Jest/Karma)
- Prettier (`.prettierrc` có sẵn)

## Folder Structure
```
src/app/
├── core/
│   ├── auth/          # AuthService, AuthGuard, PlanGuard, AuthInterceptor
│   ├── http/          # ApiService, ErrorInterceptor
│   └── models/        # TypeScript interfaces: User, Project, Entry, Subscriber
├── features/
│   ├── auth/          # login/, register/
│   ├── dashboard/     # dashboard/
│   ├── entries/       # entry-list/, entry-editor/
│   ├── subscribers/   # subscriber-list/
│   ├── settings/      # appearance/, billing/, custom-domain/
│   └── widget/        # widget-setup/
├── public/            # changelog-page/, entry-card/ (không cần auth)
└── shared/
    ├── components/    # badge, button, confirm-dialog, empty-state
    └── layout/        # app-shell, sidebar, topbar
```

## Coding Rules — KHÔNG ĐƯỢC VI PHẠM

### Standalone components — bắt buộc
```typescript
// ✅
@Component({
  selector: 'app-entry-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './entry-list.html',
})
export class EntryList { }

// ❌ NgModule-based components
```

### Services — dùng inject() + Signals
```typescript
// ✅
@Injectable({ providedIn: 'root' })
export class EntryService {
  private api = inject(ApiService);
  entries = signal<Entry[]>([]);
  isLoading = signal(false);

  loadEntries(projectId: string) {
    this.isLoading.set(true);
    return this.api.get<Entry[]>(`/entries?projectId=${projectId}`).pipe(
      tap(data => this.entries.set(data)),
      finalize(() => this.isLoading.set(false))
    );
  }
}

// ❌ BehaviorSubject khi Signals đủ dùng
```

### Reactive Forms cho forms có validation
```typescript
// ✅
form = new FormGroup({
  title: new FormControl('', [Validators.required, Validators.maxLength(300)]),
  content: new FormControl('', Validators.required),
});

// ❌ Template-driven forms [(ngModel)]
```

### Environment config — luôn dùng environment file
```typescript
// src/environments/environment.ts
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5289/api'
};

// src/environments/environment.ts (production)
export const environment = {
  production: true,
  apiUrl: 'https://api.changelogsaas.com/api'
};
```

### Routes — lazy loading bắt buộc cho features
```typescript
// ✅ app.routes.ts
export const routes: Routes = [
  { path: 'login', loadComponent: () => import('./features/auth/login/login').then(m => m.Login) },
  {
    path: 'app',
    component: AppShell,
    canActivate: [AuthGuard],
    children: [
      { path: 'entries', loadComponent: () => import('./features/entries/entry-list/entry-list').then(m => m.EntryList) },
    ]
  },
  { path: 'c/:slug', loadComponent: () => import('./public/changelog-page/changelog-page').then(m => m.ChangelogPage) },
];
```

### Unsubscribe observables
```typescript
// ✅ dùng takeUntilDestroyed()
export class MyComponent {
  private destroyRef = inject(DestroyRef);

  ngOnInit() {
    this.service.someObs$.pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(...);
  }
}
```

## app.config.ts — cần wire đủ
```typescript
export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes, withComponentInputBinding()),
    provideClientHydration(withEventReplay()),
    provideHttpClient(withInterceptors([authInterceptor, errorInterceptor]), withFetch()),
    provideBrowserGlobalErrorListeners(),
  ]
};
```

## Current State (2026-05-31)
Tất cả components/services đang là **skeleton rỗng**. Thứ tự implement theo build plan:
- ⬜ Tuần 1, Buổi 4: Angular setup (AppConfig, routes, environment, AuthService, interceptors)
- ⬜ Tuần 3: Dashboard, EntryList, EntryEditor (ngx-quill), AppShell, Sidebar
- ⬜ Tuần 4: PublicPage, WidgetSetup
- ⬜ Tuần 5: BillingPage, PlanGuard

## Skills
- FE review: `../../_claude/skills/fe-review/SKILL.md`
- Session guide: `../../_claude/skills/session-guide/SKILL.md`
- Sub-agents: `../../_claude/agents/README.md`
