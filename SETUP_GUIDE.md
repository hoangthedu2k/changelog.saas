# ChangelogSaaS Backend — Setup Guide

Hướng dẫn từng bước dựng lại backend giống dự án này: .NET 10, Clean Architecture 4 layer, CQRS với MediatR, FluentValidation, JWT, EF Core + PostgreSQL. Có sẵn module **Auth** (Register/Login/Me) và **Projects CRUD**.

---

## Mục lục

1. [Yêu cầu môi trường](#1-yêu-cầu-môi-trường)
2. [Tạo solution và 4 project](#2-tạo-solution-và-4-project)
3. [docker-compose cho PostgreSQL](#3-docker-compose-cho-postgresql)
4. [Domain layer](#4-domain-layer)
5. [Application layer](#5-application-layer)
6. [Infrastructure layer](#6-infrastructure-layer)
7. [API layer](#7-api-layer)
8. [Tạo migration và chạy](#8-tạo-migration-và-chạy)
9. [Test bằng curl/Postman](#9-test-bằng-curlpostman)
10. [Khi cần thêm feature mới — pattern CQRS](#10-khi-cần-thêm-feature-mới)

---

## 1. Yêu cầu môi trường

- .NET SDK 10
- Docker Desktop (cho PostgreSQL)
- IDE: Visual Studio 2026 / Rider / VS Code

Kiểm tra:

```bash
dotnet --version       # 10.x
docker --version
```

---
## 2. Tạo solution và 4 project

```bash
mkdir Source/be && cd Source/be

dotnet new sln -n ChangelogSaas

dotnet new classlib -n ChangelogSaas.Domain        -f net10.0
dotnet new classlib -n ChangelogSaas.Application   -f net10.0
dotnet new classlib -n ChangelogSaas.Infrastructure -f net10.0
dotnet new webapi   -n ChangelogSaas.API           -f net10.0 --use-minimal-apis

dotnet sln add ChangelogSaas.Domain ChangelogSaas.Application ChangelogSaas.Infrastructure ChangelogSaas.API
```

**Project references** (Clean Architecture: API → Application → Domain; Infrastructure → Application + Domain):

```bash
dotnet add ChangelogSaas.Application   reference ChangelogSaas.Domain
dotnet add ChangelogSaas.Infrastructure reference ChangelogSaas.Application ChangelogSaas.Domain
dotnet add ChangelogSaas.API           reference ChangelogSaas.Application ChangelogSaas.Infrastructure
```

**NuGet packages**:

```bash
# Application
dotnet add ChangelogSaas.Application package MediatR
dotnet add ChangelogSaas.Application package FluentValidation
dotnet add ChangelogSaas.Application package FluentValidation.DependencyInjectionExtensions
dotnet add ChangelogSaas.Application package Microsoft.EntityFrameworkCore       # cho IAppDbContext (DbSet<T>)

# Infrastructure
dotnet add ChangelogSaas.Infrastructure package Microsoft.EntityFrameworkCore
dotnet add ChangelogSaas.Infrastructure package Microsoft.EntityFrameworkCore.Design
dotnet add ChangelogSaas.Infrastructure package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add ChangelogSaas.Infrastructure package BCrypt.Net-Next
dotnet add ChangelogSaas.Infrastructure package System.IdentityModel.Tokens.Jwt
dotnet add ChangelogSaas.Infrastructure package Microsoft.Extensions.Options.ConfigurationExtensions

# API
dotnet add ChangelogSaas.API package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add ChangelogSaas.API package Microsoft.EntityFrameworkCore.Design          # cần cho `dotnet ef`
```

Cấu trúc cuối cùng:

```
Source/be/
├── ChangelogSaas.sln
├── ChangelogSaas.Domain/
├── ChangelogSaas.Application/
├── ChangelogSaas.Infrastructure/
└── ChangelogSaas.API/
```

---

## 3. docker-compose cho PostgreSQL

Tạo `Source/be/docker-compose.yml`:

```yaml
services:
  postgres:
    image: postgres:16
    container_name: changelogsaas-postgres
    environment:
      POSTGRES_USER: changelog
      POSTGRES_PASSWORD: changelog
      POSTGRES_DB: changelogsaas
    ports:
      - "5432:5432"
    volumes:
      - postgres-data:/var/lib/postgresql/data

volumes:
  postgres-data:
```

```bash
docker-compose up -d
```

---

## 4. Domain layer

Domain chỉ chứa entity, enum, exception. **Không** depend vào Application/Infrastructure/EF.

### 4.1 BaseEntity

`ChangelogSaas.Domain/Entities/BaseEntity.cs`:

```csharp
namespace ChangelogSaas.Domain.Entities
{
    public class BaseEntity
    {
        public Guid Id { get; protected set; }
        public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    }
}
```

### 4.2 User entity

`ChangelogSaas.Domain/Entities/User.cs`:

```csharp
namespace ChangelogSaas.Domain.Entities
{
    public class User : BaseEntity
    {
        public string Email { get; private set; } = "";
        public string PasswordHash { get; private set; } = "";
        public string? DisplayName { get; private set; }

        public static User Create(string email, string passwordHash, string? displayName = null)
        {
            return new User
            {
                Id = Guid.NewGuid(),
                Email = email.Trim().ToLowerInvariant(),
                PasswordHash = passwordHash,
                DisplayName = displayName,
                CreatedAt = DateTime.UtcNow
            };
        }

        public void UpdateDisplayName(string displayName) => DisplayName = displayName;
    }
}
```

> **Pattern**: factory method (`Create`) + private setters → entity tự bảo vệ invariant. Không bao giờ `new User { ... }` từ ngoài.

### 4.3 Project entity + WidgetPosition enum

`ChangelogSaas.Domain/Enums/WidgetPosition.cs`:

```csharp
namespace ChangelogSaas.Domain.Enums
{
    public enum WidgetPosition { bl, br, tl, tr }
}
```

`ChangelogSaas.Domain/Entities/Project.cs`:

```csharp
using ChangelogSaas.Domain.Enums;

namespace ChangelogSaas.Domain.Entities
{
    public class Project : BaseEntity
    {
        public Guid UserId { get; private set; }
        public string Name { get; private set; } = "";
        public string Slug { get; private set; } = "";
        public string? CustomDomain { get; private set; }
        public string AccentColor { get; private set; } = "#6366f1";
        public bool IsPublic { get; private set; } = true;
        public WidgetPosition WidgetPosition { get; private set; }

        public static Project Create(Guid userId, string name, string slug)
        {
            return new Project
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = name,
                Slug = slug.ToLowerInvariant(),
                CreatedAt = DateTime.UtcNow
            };
        }

        public void UpdateSettings(string name, string color)
        {
            Name = name;
            AccentColor = color;
        }

        public void SetCustomDomain(string? domain) => CustomDomain = domain;
    }
}
```

### 4.4 Exceptions

`ChangelogSaas.Domain/Exceptions/DomainException.cs`:

```csharp
namespace ChangelogSaas.Domain.Exceptions
{
    public class DomainException(string message) : Exception(message) { }
}
```

`ChangelogSaas.Domain/Exceptions/ValidationException.cs`:

```csharp
namespace ChangelogSaas.Domain.Exceptions
{
    public class ValidationException(string message) : DomainException(message) { }
}
```

`ChangelogSaas.Domain/Exceptions/NotFoundException.cs`:

```csharp
namespace ChangelogSaas.Domain.Exceptions
{
    public class NotFoundException(string entity, object key)
        : DomainException($"{entity} with id '{key}' was not found.") { }
}
```

---
## 5. Application layer

### 5.1 Interfaces

`ChangelogSaas.Application/Interfaces/IAppDbContext.cs` — abstraction cho EF context:

```csharp
using ChangelogSaas.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChangelogSaas.Application.Interfaces
{
    public interface IAppDbContext
    {
        DbSet<User> Users { get; }
        DbSet<Project> Projects { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
```

`ChangelogSaas.Application/Interfaces/IPasswordHasher.cs`:

```csharp
namespace ChangelogSaas.Application.Interfaces
{
    public interface IPasswordHasher
    {
        string Hash(string password);
        bool Verify(string password, string hash);
    }
}
```

`ChangelogSaas.Application/Interfaces/ITokenService.cs`:

```csharp
using ChangelogSaas.Domain.Entities;

namespace ChangelogSaas.Application.Interfaces
{
    public interface ITokenService
    {
        (string Token, DateTime ExpiresAt) GenerateToken(User user);
    }
}
```

> **Tại sao đặt interface ở Application?** Application là tầng "use case" — nó định nghĩa cần gì (interface), Infrastructure mới implement bằng EF/BCrypt/JWT. Đây là Dependency Inversion: layer cao không phụ thuộc layer thấp.

### 5.2 ValidationBehavior — chạy validator tự động trước handler

`ChangelogSaas.Application/Common/Behaviors/ValidationBehavior.cs`:

```csharp
using FluentValidation;
using MediatR;
using DomainValidationException = ChangelogSaas.Domain.Exceptions.ValidationException;

namespace ChangelogSaas.Application.Common.Behaviors
{
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
            => _validators = validators;

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
        {
            if (!_validators.Any()) return await next();

            var context = new ValidationContext<TRequest>(request);
            var failures = (await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, ct))))
                .SelectMany(r => r.Errors)
                .Where(f => f is not null)
                .ToList();

            if (failures.Count > 0)
                throw new DomainValidationException(string.Join(" ", failures.Select(f => f.ErrorMessage)));

            return await next();
        }
    }
}
```

### 5.3 AuthResult DTO

`ChangelogSaas.Application/Common/AuthResult.cs`:

```csharp
namespace ChangelogSaas.Application.Common
{
    public sealed record AuthResult(
        string Token,
        DateTime ExpiresAt,
        Guid UserId,
        string Email,
        string? DisplayName);
}
```

### 5.4 DependencyInjection (Application)

`ChangelogSaas.Application/DependencyInjection.cs`:

```csharp
using System.Reflection;
using ChangelogSaas.Application.Common.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ChangelogSaas.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
            services.AddValidatorsFromAssembly(assembly);
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            return services;
        }
    }
}
```

### 5.5 Auth — Register

`ChangelogSaas.Application/Auth/Commands/Register/RegisterCommand.cs`:

```csharp
using ChangelogSaas.Application.Common;
using MediatR;

namespace ChangelogSaas.Application.Auth.Commands.Register
{
    public sealed record RegisterCommand(string Email, string Password, string? DisplayName) : IRequest<AuthResult>;
}
```

`ChangelogSaas.Application/Auth/Commands/Register/RegisterCommandValidator.cs`:

```csharp
using FluentValidation;

namespace ChangelogSaas.Application.Auth.Commands.Register
{
    public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        public RegisterCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email format is invalid.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters.");
        }
    }
}
```

`ChangelogSaas.Application/Auth/Commands/Register/RegisterCommandHandler.cs`:

```csharp
using ChangelogSaas.Application.Common;
using ChangelogSaas.Application.Interfaces;
using ChangelogSaas.Domain.Entities;
using ChangelogSaas.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChangelogSaas.Application.Auth.Commands.Register
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResult>
    {
        private readonly IAppDbContext _db;
        private readonly IPasswordHasher _hasher;
        private readonly ITokenService _tokens;

        public RegisterCommandHandler(IAppDbContext db, IPasswordHasher hasher, ITokenService tokens)
        {
            _db = db; _hasher = hasher; _tokens = tokens;
        }

        public async Task<AuthResult> Handle(RegisterCommand request, CancellationToken ct)
        {
            var normalized = request.Email.Trim().ToLowerInvariant();

            var exists = await _db.Users.AnyAsync(u => u.Email == normalized, ct);
            if (exists) throw new ValidationException("Email already exists.");

            var user = User.Create(normalized, _hasher.Hash(request.Password), request.DisplayName);
            _db.Users.Add(user);
            await _db.SaveChangesAsync(ct);

            var (token, expiresAt) = _tokens.GenerateToken(user);
            return new AuthResult(token, expiresAt, user.Id, user.Email, user.DisplayName);
        }
    }
}
```

### 5.6 Auth — Login

`ChangelogSaas.Application/Auth/Commands/Login/LoginCommand.cs`:

```csharp
using ChangelogSaas.Application.Common;
using MediatR;

namespace ChangelogSaas.Application.Auth.Commands.Login
{
    public sealed record LoginCommand(string Email, string Password) : IRequest<AuthResult>;
}
```

`ChangelogSaas.Application/Auth/Commands/Login/LoginCommandValidator.cs`:

```csharp
using FluentValidation;

namespace ChangelogSaas.Application.Auth.Commands.Login
{
    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required.");
            RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required.");
        }
    }
}
```

`ChangelogSaas.Application/Auth/Commands/Login/LoginCommandHandler.cs`:

```csharp
using ChangelogSaas.Application.Common;
using ChangelogSaas.Application.Interfaces;
using ChangelogSaas.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChangelogSaas.Application.Auth.Commands.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResult>
    {
        private readonly IAppDbContext _db;
        private readonly IPasswordHasher _hasher;
        private readonly ITokenService _tokens;

        public LoginCommandHandler(IAppDbContext db, IPasswordHasher hasher, ITokenService tokens)
        {
            _db = db; _hasher = hasher; _tokens = tokens;
        }

        public async Task<AuthResult> Handle(LoginCommand request, CancellationToken ct)
        {
            var normalized = request.Email.Trim().ToLowerInvariant();

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == normalized, ct);
            if (user is null || !_hasher.Verify(request.Password, user.PasswordHash))
                throw new ValidationException("Invalid email or password.");

            var (token, expiresAt) = _tokens.GenerateToken(user);
            return new AuthResult(token, expiresAt, user.Id, user.Email, user.DisplayName);
        }
    }
}
```

### 5.7 Project DTOs

`ChangelogSaas.Application/Common/DTOs/Project/CreateProjectRequest.cs`:

```csharp
using ChangelogSaas.Domain.Enums;

namespace ChangelogSaas.Application.Common.DTOs.Project
{
    public class CreateProjectRequest
    {
        public Guid UserId { get; set; }
        public string Name { get; set; } = "";
        public string Slug { get; set; } = "";
        public string? CustomDomain { get; set; }
        public string AccentColor { get; set; } = "#6366f1";
        public bool IsPublic { get; set; } = true;
        public WidgetPosition WidgetPosition { get; set; } = WidgetPosition.tl;
    }
}
```

`ChangelogSaas.Application/Common/DTOs/Project/UpdateProjectRequest.cs`:

```csharp
using ChangelogSaas.Domain.Enums;

namespace ChangelogSaas.Application.Common.DTOs.Project
{
    public class UpdateProjectRequest
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; } = "";
        public string Slug { get; set; } = "";
        public string? CustomDomain { get; set; }
        public string AccentColor { get; set; } = "#6366f1";
        public bool IsPublic { get; set; } = true;
        public WidgetPosition WidgetPosition { get; set; } = WidgetPosition.tl;
    }
}
```

`ChangelogSaas.Application/Common/DTOs/Project/GetProjectsRequest.cs`:

```csharp
namespace ChangelogSaas.Application.Common.DTOs.Project
{
    public class GetProjectsRequest
    {
        public string? Name { get; init; }
        public string? Slug { get; init; }
    }
}
```

`ChangelogSaas.Application/Common/DTOs/Project/ProjectDTO.cs`:

```csharp
using ChangelogSaas.Domain.Enums;

namespace ChangelogSaas.Application.Common.DTOs.Project
{
    public class ProjectDTO
    {
        public Guid UserId { get; set; }
        public string? Name { get; set; }
        public string? Slug { get; set; }
        public string? CustomDomain { get; set; }
        public string AccentColor { get; set; } = "#6366f1";
        public bool IsPublic { get; set; } = true;
        public WidgetPosition WidgetPosition { get; set; }
    }
}
```

### 5.8 Projects — Create

`ChangelogSaas.Application/Projects/Commands/CreateProjectCommand/CreateProjectCommand.cs`:

```csharp
using ChangelogSaas.Application.Common.DTOs.Project;
using MediatR;

namespace ChangelogSaas.Application.Projects.Commands.CreateProjectCommand
{
    public sealed record CreateProjectCommand(CreateProjectRequest Request) : IRequest<Guid>;
}
```

`ChangelogSaas.Application/Projects/Commands/CreateProjectCommand/CreateProjectCommandValidator.cs`:

```csharp
using FluentValidation;

namespace ChangelogSaas.Application.Projects.Commands.CreateProjectCommand
{
    public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
    {
        public CreateProjectCommandValidator()
        {
            RuleFor(x => x.Request.Name)
                .NotEmpty().WithMessage("Project name is required.")
                .MaximumLength(100);

            RuleFor(x => x.Request.Slug)
                .NotEmpty().WithMessage("Project slug is required.")
                .MaximumLength(100);
        }
    }
}
```

`ChangelogSaas.Application/Projects/Commands/CreateProjectCommand/CreateProjectCommandHandler.cs`:

```csharp
using ChangelogSaas.Application.Interfaces;
using ChangelogSaas.Domain.Entities;
using MediatR;

namespace ChangelogSaas.Application.Projects.Commands.CreateProjectCommand
{
    public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, Guid>
    {
        private readonly IAppDbContext _db;
        public CreateProjectCommandHandler(IAppDbContext db) => _db = db;

        public async Task<Guid> Handle(CreateProjectCommand request, CancellationToken ct)
        {
            var project = Project.Create(request.Request.UserId, request.Request.Name, request.Request.Slug);
            _db.Projects.Add(project);
            await _db.SaveChangesAsync(ct);
            return project.Id;
        }
    }
}
```

### 5.9 Projects — Update

`ChangelogSaas.Application/Projects/Commands/UpdateProjectCommand/UpdateProjectCommand.cs`:

```csharp
using ChangelogSaas.Application.Common.DTOs.Project;
using MediatR;

namespace ChangelogSaas.Application.Projects.Commands.UpdateProjectCommand
{
    public sealed record UpdateProjectCommand(UpdateProjectRequest Request) : IRequest<Guid>;
}
```

`ChangelogSaas.Application/Projects/Commands/UpdateProjectCommand/UpdateProjectCommandValidator.cs`:

```csharp
using FluentValidation;

namespace ChangelogSaas.Application.Projects.Commands.UpdateProjectCommand
{
    public class UpdateProjectCommandValidator : AbstractValidator<UpdateProjectCommand>
    {
        public UpdateProjectCommandValidator()
        {
            RuleFor(x => x.Request.Id).NotEmpty();
            RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Request.AccentColor).NotEmpty().MaximumLength(20);
            RuleFor(x => x.Request.CustomDomain)
                .MaximumLength(255)
                .When(x => !string.IsNullOrWhiteSpace(x.Request.CustomDomain));
        }
    }
}
```

`ChangelogSaas.Application/Projects/Commands/UpdateProjectCommand/UpdateProjectCommandHandler.cs`:

```csharp
using ChangelogSaas.Application.Interfaces;
using ChangelogSaas.Domain.Entities;
using ChangelogSaas.Domain.Exceptions;
using MediatR;

namespace ChangelogSaas.Application.Projects.Commands.UpdateProjectCommand
{
    public class UpdateProjectCommandHandler : IRequestHandler<UpdateProjectCommand, Guid>
    {
        private readonly IAppDbContext _db;
        public UpdateProjectCommandHandler(IAppDbContext db) => _db = db;

        public async Task<Guid> Handle(UpdateProjectCommand request, CancellationToken ct)
        {
            var project = await _db.Projects.FindAsync(new object[] { request.Request.Id }, ct);
            if (project is null) throw new NotFoundException(nameof(Project), request.Request.Id);

            project.UpdateSettings(request.Request.Name, request.Request.AccentColor);
            project.SetCustomDomain(request.Request.CustomDomain);
            await _db.SaveChangesAsync(ct);
            return project.Id;
        }
    }
}
```

### 5.10 Projects — Delete

`ChangelogSaas.Application/Projects/Commands/DeleteProjectCommand/DeleteProjectCommand.cs`:

```csharp
using MediatR;

namespace ChangelogSaas.Application.Projects.Commands.DeleteProjectCommand
{
    public sealed record DeleteProjectCommand(Guid ProjectId) : IRequest;
}
```

`ChangelogSaas.Application/Projects/Commands/DeleteProjectCommand/DeleteProjectCommandValidator.cs`:

```csharp
using FluentValidation;

namespace ChangelogSaas.Application.Projects.Commands.DeleteProjectCommand
{
    public class DeleteProjectCommandValidator : AbstractValidator<DeleteProjectCommand>
    {
        public DeleteProjectCommandValidator()
            => RuleFor(x => x.ProjectId).NotEmpty();
    }
}
```

`ChangelogSaas.Application/Projects/Commands/DeleteProjectCommand/DeleteProjectCommandHandler.cs`:

```csharp
using ChangelogSaas.Application.Interfaces;
using ChangelogSaas.Domain.Entities;
using ChangelogSaas.Domain.Exceptions;
using MediatR;

namespace ChangelogSaas.Application.Projects.Commands.DeleteProjectCommand
{
    public class DeleteProjectCommandHandler : IRequestHandler<DeleteProjectCommand>
    {
        private readonly IAppDbContext _db;
        public DeleteProjectCommandHandler(IAppDbContext db) => _db = db;

        public async Task Handle(DeleteProjectCommand request, CancellationToken ct)
        {
            var project = await _db.Projects.FindAsync(new object[] { request.ProjectId }, ct);
            if (project is null) throw new NotFoundException(nameof(Project), request.ProjectId);

            _db.Projects.Remove(project);
            await _db.SaveChangesAsync(ct);
        }
    }
}
```

### 5.11 Projects — Get list

`ChangelogSaas.Application/Projects/Queries/GetProjectsQuery/GetProjectsQuery.cs`:

```csharp
using ChangelogSaas.Application.Common.DTOs.Project;
using MediatR;

namespace ChangelogSaas.Application.Projects.Queries.GetProjectsQuery
{
    public sealed record GetProjectsQuery(GetProjectsRequest Request) : IRequest<List<ProjectDTO>>;
}
```

`ChangelogSaas.Application/Projects/Queries/GetProjectsQuery/GetProjectsQueryHandler.cs`:

```csharp
using ChangelogSaas.Application.Common.DTOs.Project;
using ChangelogSaas.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChangelogSaas.Application.Projects.Queries.GetProjectsQuery
{
    public class GetProjectsQueryHandler : IRequestHandler<GetProjectsQuery, List<ProjectDTO>>
    {
        private readonly IAppDbContext _db;
        public GetProjectsQueryHandler(IAppDbContext db) => _db = db;

        public async Task<List<ProjectDTO>> Handle(GetProjectsQuery request, CancellationToken ct)
        {
            var query = _db.Projects.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Request.Name))
                query = query.Where(p => p.Name.Contains(request.Request.Name!));

            if (!string.IsNullOrWhiteSpace(request.Request.Slug))
                query = query.Where(p => p.Slug.Contains(request.Request.Slug!));

            return await query
                .Select(p => new ProjectDTO
                {
                    UserId = p.UserId,
                    Name = p.Name,
                    Slug = p.Slug,
                    CustomDomain = p.CustomDomain,
                    AccentColor = p.AccentColor,
                    IsPublic = p.IsPublic,
                    WidgetPosition = p.WidgetPosition
                })
                .ToListAsync(ct);
        }
    }
}
```

---
## 6. Infrastructure layer

### 6.1 EF Configuration (Fluent API)

`ChangelogSaas.Infrastructure/Persistence/Configurations/UserConfiguration.cs`:

```csharp
using ChangelogSaas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChangelogSaas.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Email).IsRequired().HasMaxLength(256);
        builder.HasIndex(x => x.Email).IsUnique();
        builder.Property(x => x.PasswordHash).IsRequired();
        builder.Property(x => x.DisplayName).HasMaxLength(100);
    }
}
```

`ChangelogSaas.Infrastructure/Persistence/Configurations/ProjectConfiguration.cs`:

```csharp
using ChangelogSaas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChangelogSaas.Infrastructure.Persistence.Configurations;

public sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Slug).IsRequired().HasMaxLength(100);
        builder.HasIndex(x => x.Slug).IsUnique();
        builder.Property(x => x.AccentColor).HasMaxLength(20);
        builder.Property(x => x.CustomDomain).HasMaxLength(253);
    }
}
```

> **Quy tắc**: cấu hình EF qua Fluent API trong `OnModelCreating` (hoặc `IEntityTypeConfiguration<T>`). Không dùng Data Annotations như `[Required]`, `[MaxLength]` — Domain entity phải sạch, không biết về EF.

### 6.2 AppDbContext

`ChangelogSaas.Infrastructure/Persistence/AppDbContext.cs`:

```csharp
using ChangelogSaas.Application.Interfaces;
using ChangelogSaas.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChangelogSaas.Infrastructure.Persistence
{
    public class AppDbContext : DbContext, IAppDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Project> Projects { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
```

### 6.3 BCrypt password hasher

`ChangelogSaas.Infrastructure/Auth/BcryptPasswordHasher.cs`:

```csharp
using ChangelogSaas.Application.Interfaces;

namespace ChangelogSaas.Infrastructure.Auth
{
    public class BcryptPasswordHasher : IPasswordHasher
    {
        private const int WorkFactor = 12;

        public string Hash(string password)
            => BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

        public bool Verify(string password, string hash)
        {
            try { return BCrypt.Net.BCrypt.Verify(password, hash); }
            catch (BCrypt.Net.SaltParseException) { return false; }
        }
    }
}
```

### 6.4 JWT options + token service

`ChangelogSaas.Infrastructure/Auth/JwtOptions.cs`:

```csharp
namespace ChangelogSaas.Infrastructure.Auth
{
    public class JwtOptions
    {
        public string Key { get; set; } = "";
        public string Issuer { get; set; } = "";
        public string Audience { get; set; } = "";
        public int ExpiryMinutes { get; set; } = 1440;
    }
}
```

`ChangelogSaas.Infrastructure/Auth/JwtTokenService.cs`:

```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ChangelogSaas.Application.Interfaces;
using ChangelogSaas.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ChangelogSaas.Infrastructure.Auth
{
    public class JwtTokenService : ITokenService
    {
        private readonly JwtOptions _options;

        public JwtTokenService(IOptions<JwtOptions> options)
        {
            _options = options.Value;
            if (string.IsNullOrWhiteSpace(_options.Key) || _options.Key.Length < 32)
                throw new InvalidOperationException("Jwt:Key must be set and at least 32 characters long.");
        }

        public (string Token, DateTime ExpiresAt) GenerateToken(User user)
        {
            var now = DateTime.UtcNow;
            var expiresAt = now.AddMinutes(_options.ExpiryMinutes);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            if (!string.IsNullOrEmpty(user.DisplayName))
                claims.Add(new Claim("name", user.DisplayName));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                notBefore: now,
                expires: expiresAt,
                signingCredentials: creds);

            return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
        }
    }
}
```

### 6.5 DependencyInjection (Infrastructure)

`ChangelogSaas.Infrastructure/DependencyInjection.cs`:

```csharp
using ChangelogSaas.Application.Interfaces;
using ChangelogSaas.Infrastructure.Auth;
using ChangelogSaas.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChangelogSaas.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString, npgsql =>
                    npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)
                          .MigrationsHistoryTable("__EFMigrationsHistory")));

            services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

            services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
            services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
            services.AddSingleton<ITokenService, JwtTokenService>();

            return services;
        }
    }
}
```

---
## 7. API layer

### 7.1 ExceptionMiddleware — map exception thành ProblemDetails

`ChangelogSaas.API/Middleware/ExceptionMiddleware.cs`:

```csharp
using System.Text.Json;
using ChangelogSaas.Domain.Exceptions;

namespace ChangelogSaas.API.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next; _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try { await _next(context); }
            catch (ValidationException ex)
            {
                await WriteProblemAsync(context, StatusCodes.Status400BadRequest, "Validation failed", ex.Message);
            }
            catch (NotFoundException ex)
            {
                await WriteProblemAsync(context, StatusCodes.Status404NotFound, "Not found", ex.Message);
            }
            catch (DomainException ex)
            {
                await WriteProblemAsync(context, StatusCodes.Status400BadRequest, "Domain rule violated", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");
                await WriteProblemAsync(context, StatusCodes.Status500InternalServerError, "Internal server error", "An unexpected error occurred.");
            }
        }

        private static Task WriteProblemAsync(HttpContext context, int statusCode, string title, string detail)
        {
            if (context.Response.HasStarted) return Task.CompletedTask;

            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";

            var payload = JsonSerializer.Serialize(new
            {
                type = $"https://httpstatuses.io/{statusCode}",
                title,
                status = statusCode,
                detail
            });
            return context.Response.WriteAsync(payload);
        }
    }
}
```

### 7.2 Auth endpoints

`ChangelogSaas.API/Endpoints/AuthEndpoints.cs`:

```csharp
using System.Security.Claims;
using ChangelogSaas.Application.Auth.Commands.Login;
using ChangelogSaas.Application.Auth.Commands.Register;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ChangelogSaas.API.Endpoints
{
    public static class AuthEndpoints
    {
        public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
        {
            var g = app.MapGroup("/api/auth");
            g.MapPost("/register", Register);
            g.MapPost("/login", Login);
            g.MapGet("/me", Me).RequireAuthorization();
            return app;
        }

        private static async Task<IResult> Register([FromBody] RegisterCommand cmd, ISender sender, CancellationToken ct)
            => Results.Ok(await sender.Send(cmd, ct));

        private static async Task<IResult> Login([FromBody] LoginCommand cmd, ISender sender, CancellationToken ct)
            => Results.Ok(await sender.Send(cmd, ct));

        private static IResult Me(ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue("sub") ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = user.FindFirstValue("email") ?? user.FindFirstValue(ClaimTypes.Email);
            var name = user.FindFirstValue("name");
            return Results.Ok(new { userId, email, displayName = name });
        }
    }
}
```

### 7.3 Project endpoints

`ChangelogSaas.API/Endpoints/ProjectEndpoints.cs`:

```csharp
using ChangelogSaas.Application.Common.DTOs.Project;
using ChangelogSaas.Application.Projects.Commands.CreateProjectCommand;
using ChangelogSaas.Application.Projects.Commands.DeleteProjectCommand;
using ChangelogSaas.Application.Projects.Commands.UpdateProjectCommand;
using ChangelogSaas.Application.Projects.Queries.GetProjectsQuery;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ChangelogSaas.API.Endpoints
{
    public static class ProjectEndpoints
    {
        public static IEndpointRouteBuilder MapProjectEndpoints(this IEndpointRouteBuilder app)
        {
            var g = app.MapGroup("/api/projects").RequireAuthorization();

            g.MapGet("/", GetList);
            g.MapPost("/", Create);
            g.MapPut("/{projectId:guid}", Update);
            g.MapDelete("/{projectId:guid}", Delete);

            return app;
        }

        private static async Task<IResult> GetList([AsParameters] GetProjectsRequest request, ISender sender, CancellationToken ct)
            => Results.Ok(await sender.Send(new GetProjectsQuery(request), ct));

        private static async Task<IResult> Create([FromBody] CreateProjectRequest request, ISender sender, CancellationToken ct)
            => Results.Ok(await sender.Send(new CreateProjectCommand(request), ct));

        private static async Task<IResult> Update(
            [FromRoute] Guid projectId,
            [FromBody] UpdateProjectRequest request,
            ISender sender,
            CancellationToken ct)
        {
            request.Id = projectId;
            return Results.Ok(await sender.Send(new UpdateProjectCommand(request), ct));
        }

        private static async Task<IResult> Delete([FromRoute] Guid projectId, ISender sender, CancellationToken ct)
        {
            await sender.Send(new DeleteProjectCommand(projectId), ct);
            return Results.NoContent();
        }
    }
}
```

### 7.4 appsettings.json

`ChangelogSaas.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=changelogsaas;Username=changelog;Password=changelog"
  },
  "Jwt": {
    "Key": "REPLACE_WITH_AT_LEAST_32_CHARACTERS_RANDOM_STRING",
    "Issuer": "ChangelogSaas",
    "Audience": "ChangelogSaas.Client",
    "ExpiryMinutes": 1440
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

> **Lưu ý**: `Jwt:Key` phải >= 32 ký tự. Trong production nên đặt qua environment variable / user secrets, không commit.

### 7.5 Program.cs

`ChangelogSaas.API/Program.cs`:

```csharp
using System.Text;
using ChangelogSaas.API.Endpoints;
using ChangelogSaas.API.Middleware;
using ChangelogSaas.Application;
using ChangelogSaas.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();

var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"] ?? throw new InvalidOperationException("Jwt:Key is missing.");
var jwtIssuer = jwtSection["Issuer"];
var jwtAudience = jwtSection["Audience"];

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapProjectEndpoints();

app.Run();
```

> **Thứ tự middleware quan trọng**: `ExceptionMiddleware` đầu tiên (bắt mọi exception phía sau) → `Authentication` trước `Authorization` → `Map*Endpoints` cuối cùng.

---
## 8. Tạo migration và chạy

```bash
# Cài tool 1 lần (nếu chưa có)
dotnet tool install --global dotnet-ef

# Restore + build kiểm tra
dotnet restore
dotnet build

# Tạo migration đầu tiên
dotnet ef migrations add InitialDbContext `
    --project ChangelogSaas.Infrastructure `
    --startup-project ChangelogSaas.API

# Apply lên database
dotnet ef database update `
    --project ChangelogSaas.Infrastructure `
    --startup-project ChangelogSaas.API

# Chạy API
dotnet run --project ChangelogSaas.API/ChangelogSaas.API.csproj
```

API mặc định lắng nghe `http://localhost:5289` (xem `Properties/launchSettings.json`).

**Mỗi lần đổi entity / EF config**:

```bash
dotnet ef migrations add <TenMoTa> --project ChangelogSaas.Infrastructure --startup-project ChangelogSaas.API
dotnet ef database update --project ChangelogSaas.Infrastructure --startup-project ChangelogSaas.API
```

---

## 9. Test bằng curl/Postman

### 9.1 Register

```bash
curl -X POST http://localhost:5289/api/auth/register `
  -H "Content-Type: application/json" `
  -d '{"email":"a@b.com","password":"password123","displayName":"Alice"}'
```

Response:

```json
{
  "token": "eyJhbGciOi...",
  "expiresAt": "2026-06-05T...",
  "userId": "...",
  "email": "a@b.com",
  "displayName": "Alice"
}
```

### 9.2 Login

```bash
curl -X POST http://localhost:5289/api/auth/login `
  -H "Content-Type: application/json" `
  -d '{"email":"a@b.com","password":"password123"}'
```

### 9.3 Me (cần Bearer token)

```bash
curl http://localhost:5289/api/auth/me `
  -H "Authorization: Bearer <TOKEN>"
```

### 9.4 Create project

```bash
curl -X POST http://localhost:5289/api/projects `
  -H "Authorization: Bearer <TOKEN>" `
  -H "Content-Type: application/json" `
  -d '{"userId":"<USER_ID>","name":"My Product","slug":"my-product","accentColor":"#6366f1"}'
```

### 9.5 List projects (filter qua query string)

```bash
curl "http://localhost:5289/api/projects?name=product" -H "Authorization: Bearer <TOKEN>"
```

### 9.6 Update project

```bash
curl -X PUT http://localhost:5289/api/projects/<PROJECT_ID> `
  -H "Authorization: Bearer <TOKEN>" `
  -H "Content-Type: application/json" `
  -d '{"name":"New Name","accentColor":"#ff0000","slug":"my-product"}'
```

### 9.7 Delete project

```bash
curl -X DELETE http://localhost:5289/api/projects/<PROJECT_ID> -H "Authorization: Bearer <TOKEN>"
```

---

## 10. Khi cần thêm feature mới — pattern CQRS

Mỗi use case = 1 folder, 3 file. Ví dụ thêm "publish entry":

```
ChangelogSaas.Application/Entries/Commands/PublishEntry/
├── PublishEntryCommand.cs            # record : IRequest<TResponse>
├── PublishEntryCommandValidator.cs   # AbstractValidator<PublishEntryCommand>
└── PublishEntryCommandHandler.cs     # IRequestHandler<PublishEntryCommand, TResponse>
```

Sau đó endpoint chỉ cần:

```csharp
g.MapPost("/{id:guid}/publish", async ([FromRoute] Guid id, ISender sender, CancellationToken ct)
    => Results.Ok(await sender.Send(new PublishEntryCommand(id), ct)));
```

**Quy tắc bắt buộc** (trích CLAUDE.md):

| Rule | Đúng | Sai |
|---|---|---|
| DateTime | `DateTime.UtcNow` | `DateTime.Now` |
| Entity | factory method + private setter | public setter / public constructor |
| Interface | `public interface ICacheService` | `internal interface` |
| EF config | `OnModelCreating` / `IEntityTypeConfiguration` | Data Annotations |
| Validator | kế thừa `AbstractValidator<T>`, file đặt cạnh command | class rỗng / không kế thừa |
| Handler | inject `IAppDbContext` | inject `AppDbContext` trực tiếp |
| Endpoint | `Results.Ok(await sender.Send(...))` | gọi handler trực tiếp / validate thủ công |

**Ngoại lệ — khi đặt logic ở Infrastructure**: chỉ khi adapter đụng thư viện ngoài (BCrypt, JWT, Stripe, Redis, SMTP). Expose interface ở Application, implement ở Infrastructure.

**Flow request đầy đủ**:

```
HTTP request
  → Middleware (Exception, Auth)
  → Endpoint (parse, gọi ISender.Send)
  → MediatR pipeline
      → ValidationBehavior (chạy FluentValidation)
      → Handler (inject IAppDbContext, gọi domain method, SaveChanges)
  → Response (Results.Ok / NoContent)

Nếu Validator fail → throw ValidationException → ExceptionMiddleware → 400
Nếu NotFound       → throw NotFoundException   → ExceptionMiddleware → 404
Nếu lỗi khác       → ExceptionMiddleware       → 500 + log
```

---

## Checklist khi tạo project mới

- [ ] `dotnet new sln` + 4 project + reference đúng chiều phụ thuộc
- [ ] NuGet packages cài đủ ở từng layer
- [ ] `docker-compose up -d` — postgres chạy ok
- [ ] Domain: `BaseEntity`, entity (factory + private setter), enum, exceptions
- [ ] Application: `IAppDbContext`, `ValidationBehavior`, `DependencyInjection.AddApplication()`, ít nhất 1 use case mẫu
- [ ] Infrastructure: `AppDbContext`, EF Configurations, BCrypt, JWT, `DependencyInjection.AddInfrastructure()`
- [ ] API: `ExceptionMiddleware`, endpoints, JWT auth setup, CORS, `Program.cs` đúng thứ tự
- [ ] `appsettings.json`: ConnectionString + Jwt:Key (>= 32 ký tự)
- [ ] `dotnet build` — 0 error 0 warning
- [ ] `dotnet ef migrations add InitialDbContext` + `database update`
- [ ] Test register → login → me bằng curl → có token, decode được

Build sạch nghĩa là skeleton đã ổn. Từ đó thêm feature theo đúng pattern là xong.





