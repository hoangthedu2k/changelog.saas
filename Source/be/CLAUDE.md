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

## Architecture — Clean Architecture 4 layers

```
ChangelogSaas.Domain/          # Entities, Enums, Exceptions — no dependencies
ChangelogSaas.Application/     # Commands, Queries, Interfaces — depends Domain only
ChangelogSaas.Infrastructure/  # EF, Services, Jobs — implements Application interfaces
ChangelogSaas.API/             # Minimal API endpoints, Middleware, Program.cs
```

Dependency flow: API → Application → Domain; Infrastructure → Application + Domain.

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
        g.MapGet("/", GetAll);
        g.MapPost("/", Create);
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

Tiến độ hiện tại (2026-06-01):
- ✅ Tuần 1, Buổi 1–2: Solution setup, Domain entities, EF config
- ✅ Tuần 1, Buổi 3: EF migrations applied + JWT auth (register/login/me, BCrypt, ExceptionMiddleware)
- ⬜ Tuần 1, Buổi 4–5: Angular setup + login UI + GitHub repo
- ⬜ Tuần 2: CRUD API, Hangfire, Redis, Widget endpoint
- ⬜ Tuần 5: Stripe billing + Resend email

## Skills
- BE review: `../../_claude/skills/be-review/SKILL.md`
- Session guide: `../../_claude/skills/session-guide/SKILL.md`
- Sub-agents: `../../_claude/agents/README.md`
