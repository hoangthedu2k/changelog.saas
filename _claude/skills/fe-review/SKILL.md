# Skill: Frontend Code Review — ChangelogSaaS

## Khi nào dùng skill này
Khi user nói: "review Angular code", "kiểm tra FE", "vừa làm xong phần frontend", hoặc có vấn đề với Angular code.

## Quy trình Review

### Bước 1 — Scan Angular source
```
Source/fe/src/**/*.ts
Source/fe/src/**/*.html
Source/fe/src/**/*.scss
```

### Bước 2 — Checklist

#### Architecture
- [ ] Standalone components (không NgModule cũ)
- [ ] Signals dùng đúng: `signal()`, `computed()`, `effect()` — không dùng BehaviorSubject khi không cần
- [ ] Services inject bằng `inject()` hoặc constructor injection (nhất quán)
- [ ] Lazy loading routes (không load tất cả upfront)

#### Auth & Guards
- [ ] `AuthGuard` redirect về `/login` nếu không có token
- [ ] `AuthInterceptor` tự động đính token vào headers
- [ ] Token lưu trong `localStorage` (acceptable cho MVP, không sessionStorage)
- [ ] `AuthService` có: `login()`, `logout()`, `currentUser signal`, `isLoggedIn computed`

#### API Service
- [ ] Base URL từ environment (`environment.ts` / `environment.prod.ts`)
- [ ] Error handling trong interceptor (401 → logout, 500 → toast)
- [ ] Tất cả HTTP calls return `Observable` hoặc `Promise` (nhất quán)

#### Forms
- [ ] Reactive Forms (không Template-driven) cho forms phức tạp
- [ ] Validation messages hiển thị khi touched
- [ ] Submit button disabled khi form invalid

#### Performance
- [ ] `OnPush` change detection cho components không cần auto-detect
- [ ] Unsubscribe observables (dùng `takeUntilDestroyed()` với .NET inject)
- [ ] Không subscribe trong template (dùng `async` pipe hoặc signals)

### Bước 3 — Phân loại (🔴/🟡/🟢) và sửa

## Angular Patterns Chuẩn cho project này

### Service chuẩn
```typescript
@Injectable({ providedIn: 'root' })
export class ProjectService {
  private api = inject(HttpClient);
  private baseUrl = inject(API_BASE_URL);
  
  projects = signal<Project[]>([]);
  activeProject = signal<Project | null>(null);
  
  loadProjects() {
    return this.api.get<Project[]>(`${this.baseUrl}/projects`).pipe(
      tap(projects => this.projects.set(projects))
    );
  }
}
```

### Component chuẩn (standalone)
```typescript
@Component({
  selector: 'app-entry-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `...`
})
export class EntryListComponent {
  private entryService = inject(EntryService);
  entries = this.entryService.entries;
}
```

### Environment config
```typescript
// environment.ts
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000/api'
};
```
