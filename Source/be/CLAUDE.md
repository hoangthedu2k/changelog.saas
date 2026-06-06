# ChangelogSaaS — Backend CLAUDE.md

Backend context. Đọc trước khi làm bất cứ thứ gì trong `Source/be/`.

## Build & Run

```bash
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
  RegisterCommand.cs            # record : IRequest<TResponse>
  RegisterCommandHandler.cs     # IRequestHandler — inject IAppDbContext, IPasswordHasher, ITokenService...
  RegisterCommandValidator.cs   # AbstractValidator<RegisterCommand>
```

Handler dùng EF qua `IAppDbContext` (interface ở Application, `AppDbContext` ở Infrastructure implement). Không inject `AppDbContext` trực tiếp.

`ValidationBehavior` đăng ký ở `ChangelogSaas.Application/Common/Behaviors/` — tự động chạy trước mọi handler, ném `Domain.Exceptions.ValidationException` khi fail (ExceptionMiddleware map sang 400).

Endpoint chỉ gọi `ISender.Send`, không validate thủ công:
```csharp
g.MapPost("/register", async (RegisterCommand cmd, ISender sender, CancellationToken ct)
    => Results.Ok(await sender.Send(cmd, ct)));
```

DI: `Program.cs` gọi `AddApplication()` (MediatR + Validators + ValidationBehavior) trước `AddInfrastructure()` (DbContext + auth services).

**Ngoại lệ — khi nào ở Infrastructure**: chỉ dành cho adapter đụng trực tiếp thư viện ngoài (BCrypt, JWT, Stripe, Redis, SMTP). Expose interface ở Application, implement ở Infrastructure. Không đặt use case logic ở đây.

## Coding Rules — KHÔNG ĐƯỢC VI PHẠM

### DateTime — luôn UTC
```csharp
CreatedAt = DateTime.UtcNow   // ✅
CreatedAt = DateTime.Now       // ❌
```

### Entity Pattern — factory method + private setters
```csharp
// ✅
public class MyEntity : BaseEntity {
    public string Name { get; private set; } = "";
    public static MyEntity Create(string name) => new() {
        Id = Guid.NewGuid(), Name = name, CreatedAt = DateTime.UtcNow
    };
}
// ❌ public setters / public constructor
```

### Access Modifiers
```csharp
public interface ICacheService { }   // ✅ interfaces phải public
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
        g.MapPost("/", async (CreateEntryCommand cmd, ISender sender, CancellationToken ct)
            => Results.Ok(await sender.Send(cmd, ct)));
        return app;
    }
}
```

### Using Statements
- Xóa unused `using` — .NET 10 ImplicitUsings: không cần `using System;`, `using System.Collections.Generic;`

## Database

- PostgreSQL via Docker Compose (port 5432)
- EF Core 9 với Npgsql provider
- Connection string: `appsettings.json` → `ConnectionStrings:DefaultConnection`
- JWT config: `appsettings.json` → `Jwt:Key/Issuer/Audience/ExpiryMinutes`

## Testing

xUnit với coverlet. Test project: `ChangelogSaas.Tests/`.

## Build Plan Progress

Tiến độ hiện tại (2026-06-05):
- ✅ Tuần 1, Buổi 1–2: Solution setup, Domain entities, EF config
- ✅ Tuần 1, Buổi 3: EF migrations applied + JWT auth (register/login/me, BCrypt, ExceptionMiddleware)
- ✅ Tuần 1, Buổi 3.5: Refactor Auth sang CQRS + MediatR + FluentValidation (xoá `AuthService`, `IAuthService`)
- ⬜ Tuần 1, Buổi 4–5: Angular setup + login UI + GitHub repo
- ✅ Tuần 2, Buổi 1–2: Project CRUD + Entry CRUD (CQRS pattern, Commands/Queries/Validators)
- ✅ Tuần 2, Buổi 3: Widget public endpoint `GET /api/widget/{slug}` — no auth, tested end-to-end
- ✅ Tuần 2, Buổi 4: Redis caching (widget TTL 5 phút, invalidate on publish) + Hangfire setup (PostgreSQL storage, dashboard /hangfire) + Subscriber endpoints (subscribe/confirm/unsubscribe)
- ⬜ Tuần 5: Stripe billing + Resend email

## Skills
- BE review: `../../_claude/skills/be-review/SKILL.md`
- Session guide: `../../_claude/skills/session-guide/SKILL.md`
- Sub-agents: `../../_claude/agents/README.md`
