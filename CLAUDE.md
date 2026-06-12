# ChangelogSaaS — CLAUDE.md

Context tổng hợp cho toàn bộ project. Đọc trước khi làm bất cứ thứ gì.

- Làm việc trong `Source/be/` → xem thêm `Source/be/CLAUDE.md`
- Làm việc trong `Source/fe/` → xem thêm `Source/fe/CLAUDE.md`

## Language Rules — KHÔNG ĐƯỢC VI PHẠM

All code output must be in **English only**:
- UI strings, labels, button text, banners, error messages → English
- Code comments → English
- Variable names, function names → English

Conversational replies to the user may be in Vietnamese. But **nothing written into source files** (`.ts`, `.html`, `.scss`, `.cs`, etc.) should contain Vietnamese.

## Skills
- BE review: `_claude/skills/be-review/SKILL.md`
- FE review: `_claude/skills/fe-review/SKILL.md`
- Session guide: `_claude/skills/session-guide/SKILL.md`
- Sub-agents: `_claude/agents/README.md`

---

# Backend — .NET 10 / Clean Architecture / CQRS

## Build & Run

```bash
cd Source/be

# Start PostgreSQL + Redis
docker-compose up -d

# Restore + build
dotnet restore && dotnet build

# Run API (http://localhost:5289)
dotnet run --project ChangelogSaas.API/ChangelogSaas.API.csproj

# EF Migrations
dotnet ef migrations add <Name> --project ChangelogSaas.Infrastructure --startup-project ChangelogSaas.API
dotnet ef database update --project ChangelogSaas.Infrastructure --startup-project ChangelogSaas.API

# Tests
dotnet test
dotnet test --filter "ClassName=EntryServiceTests"
```

## Architecture — Clean Architecture 4 layers + CQRS (MediatR)

```
ChangelogSaas.Domain/          # Entities, Enums, Exceptions — no dependencies
ChangelogSaas.Application/     # Commands/Queries (MediatR), Validators (FluentValidation), Interfaces — depends Domain only
ChangelogSaas.Infrastructure/  # EF (AppDbContext : IAppDbContext), Services, Jobs — implements Application interfaces
ChangelogSaas.API/             # Minimal API endpoints, Middleware, Program.cs
```

Dependency flow: API → Application → Domain; Infrastructure → Application + Domain.

### Use case pattern — CQRS + MediatR (KHÔNG dùng service-based)

Mỗi use case = 1 folder gồm `Command/Query` + `Handler` + `Validator`. Đặt **toàn bộ ở Application**, không tạo service wrapper trong Infrastructure.

```
ChangelogSaas.Application/Auth/Commands/Register/
  RegisterCommand.cs
  RegisterCommandHandler.cs
  RegisterCommandValidator.cs
```

Handler dùng EF qua `IAppDbContext`. Không inject `AppDbContext` trực tiếp.

`ValidationBehavior` đăng ký ở `ChangelogSaas.Application/Common/Behaviors/` — tự động chạy trước mọi handler.

Endpoint chỉ gọi `ISender.Send`, không validate thủ công.

## Coding Rules BE — KHÔNG ĐƯỢC VI PHẠM

### DateTime — luôn UTC
```csharp
CreatedAt = DateTime.UtcNow   // ✅
CreatedAt = DateTime.Now       // ❌
```

### Entity Pattern — factory method + private setters
```csharp
public class MyEntity : BaseEntity {
    public string Name { get; private set; } = "";
    public static MyEntity Create(string name) => new() {
        Id = Guid.NewGuid(), Name = name, CreatedAt = DateTime.UtcNow
    };
}
```

### Access Modifiers
```csharp
public interface ICacheService { }   // ✅
internal interface ICacheService { } // ❌
```

### EF Config — OnModelCreating only, không dùng Data Annotations
```csharp
modelBuilder.Entity<Entry>().Property(e => e.Tags).HasColumnType("text[]"); // ✅
[Column(TypeName = "text[]")] // ❌
```

### Minimal API Endpoint Pattern
```csharp
public static class EntryEndpoints {
    public static IEndpointRouteBuilder MapEntryEndpoints(this IEndpointRouteBuilder app) {
        var g = app.MapGroup("/api/entries").RequireAuthorization();
        g.MapGet("/", async (ISender sender, CancellationToken ct)
            => Results.Ok(await sender.Send(new GetEntriesQuery(), ct)));
        return app;
    }
}
```

## Database
- PostgreSQL via Docker Compose (port 5432)
- EF Core 9 với Npgsql provider
- Connection string: `appsettings.json` → `ConnectionStrings:DefaultConnection`
- JWT config: `appsettings.json` → `Jwt:Key/Issuer/Audience/ExpiryMinutes`

## BE Build Plan Progress
- ✅ Tuần 1, Buổi 1–2: Solution setup, Domain entities, EF config
- ✅ Tuần 1, Buổi 3: EF migrations + JWT auth (register/login/me, BCrypt, ExceptionMiddleware)
- ✅ Tuần 1, Buổi 3.5: Refactor Auth sang CQRS + MediatR + FluentValidation
- ✅ Tuần 2, Buổi 1–2: Project CRUD + Entry CRUD (CQRS pattern)
- ✅ Tuần 2, Buổi 3: Widget public endpoint `GET /api/widget/{slug}`
- ✅ Tuần 2, Buổi 4: Redis caching + Hangfire + Subscriber endpoints
- ⬜ Tuần 5: Stripe billing + Resend email

---

# Frontend — Angular 21 / Signals / SSR

## Build & Run

```bash
cd Source/fe/changelog-app

npm install

# Dev server (http://localhost:4200)
ng serve

# Build production (SSR)
ng build

# Run tests (vitest)
npm test
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
│   └── models/        # TypeScript interfaces
├── features/
│   ├── auth/          # login/, register/
│   ├── dashboard/
│   ├── entries/       # entry-list/, entry-editor/
│   ├── subscribers/
│   ├── settings/      # appearance/, billing/, custom-domain/
│   └── widget/
├── public/            # changelog-page/, entry-card/
└── shared/
    ├── components/    # badge, button, confirm-dialog, empty-state
    └── layout/        # app-shell, sidebar, topbar
```

## Coding Rules FE — KHÔNG ĐƯỢC VI PHẠM

### Standalone components — bắt buộc
```typescript
@Component({
  selector: 'app-entry-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './entry-list.html',
})
export class EntryList { }
```

### Services — dùng inject() + Signals
```typescript
@Injectable({ providedIn: 'root' })
export class EntryService {
  private api = inject(ApiService);
  entries = signal<Entry[]>([]);
  isLoading = signal(false);
}
```

### Reactive Forms cho forms có validation

### Environment config — luôn dùng environment file
```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5289/api'
};
```

### Routes — lazy loading bắt buộc cho features
```typescript
{ path: 'login', loadComponent: () => import('./features/auth/login/login').then(m => m.Login) }
```

### Unsubscribe — dùng takeUntilDestroyed()

## FE Build Plan Progress
- ⬜ Tuần 1, Buổi 4: Angular setup (AppConfig, routes, environment, AuthService, interceptors)
- ⬜ Tuần 3: Dashboard, EntryList, EntryEditor (ngx-quill), AppShell, Sidebar
- ⬜ Tuần 4: PublicPage, WidgetSetup
- ⬜ Tuần 5: BillingPage, PlanGuard
