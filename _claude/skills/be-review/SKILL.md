# Skill: Backend Code Review — ChangelogSaaS

## Khi nào dùng skill này
Khi user nói: "review code", "kiểm tra lại code", "vừa làm xong [buổi X]", "có vấn đề gì không", hoặc muốn check chất lượng BE code.

## Quy trình Review

### Bước 1 — Scan toàn bộ .cs files
```
Source/be/**/*.cs (exclude: /obj/, /.vs/, /bin/)
```

### Bước 2 — Checklist theo Layer

#### Domain Layer (`ChangelogSaas.Domain/`)
- [ ] Tất cả entities kế thừa `BaseEntity`
- [ ] Tất cả entities có `public` access modifier (không `internal`)
- [ ] Tất cả entities có `static Create()` factory method
- [ ] Tất cả properties dùng `private set` (không `public set`)
- [ ] Không dùng `DateTime.Now` (phải là `DateTime.UtcNow`)
- [ ] Business methods (Publish, Confirm, Unsubscribe...) nằm trong entity
- [ ] Exceptions: `DomainException` và `NotFoundException` phải `public`

#### Application Layer (`ChangelogSaas.Application/`)
- [ ] Interfaces (`ICacheService`, `IEmailService`, `IBillingService`) là `public`
- [ ] Commands/Queries implement logic (không rỗng khi đến tuần cần dùng)
- [ ] Application KHÔNG reference Infrastructure (check .csproj)

#### Infrastructure Layer (`ChangelogSaas.Infrastructure/`)
- [ ] `AppDbContext.OnModelCreating` có config cho tất cả entities
- [ ] `List<string>` columns dùng `.HasColumnType("text[]")`
- [ ] Unique indexes: Email, Slug, (ProjectId+Email cho Subscriber)
- [ ] `DependencyInjection.cs` register đủ services
- [ ] Không có `using System.Runtime.CompilerServices` thừa

#### API Layer (`ChangelogSaas.API/`)
- [ ] `Program.cs` có: AddInfrastructure, AddAuthentication, UseMiddleware, MapEndpoints
- [ ] `appsettings.json` có: ConnectionStrings, Jwt section
- [ ] Endpoints dùng `.RequireAuthorization()` cho routes cần auth
- [ ] Middleware `ExceptionMiddleware` và `RateLimitMiddleware` không rỗng (khi đến tuần implement)

### Bước 3 — Phân loại vấn đề
- 🔴 **Crash/Compile Error**: sẽ làm app không chạy được → phải sửa ngay
- 🟡 **Logic Bug**: chạy được nhưng sai → sửa trong buổi này
- 🟢 **Code Smell**: không nguy hiểm nhưng nên sửa

### Bước 4 — Sửa tự động
Sau khi list xong:
- Sửa tất cả 🔴 và 🟡 ngay lập tức (dùng Edit tool)
- 🟢: sửa nếu còn thời gian, hoặc note lại

## Pattern chuẩn để tham chiếu

### Entity chuẩn
```csharp
public class MyEntity : BaseEntity
{
    public string Name { get; private set; } = "";
    
    public static MyEntity Create(string name)
    {
        return new MyEntity
        {
            Id = Guid.NewGuid(),
            Name = name,
            CreatedAt = DateTime.UtcNow
        };
    }
    
    public void DoSomething()
    {
        // business logic here
    }
}
```

### Endpoint chuẩn (Minimal API)
```csharp
public static class MyEndpoints
{
    public static IEndpointRouteBuilder MapMyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/my").RequireAuthorization();
        group.MapGet("/", GetAll);
        group.MapPost("/", Create);
        return app;
    }
    
    private static async Task<IResult> GetAll(AppDbContext db, ClaimsPrincipal user)
    {
        // ...
        return Results.Ok(items);
    }
}
```
