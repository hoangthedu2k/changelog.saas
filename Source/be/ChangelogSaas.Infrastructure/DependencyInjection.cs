using ChangelogSaas.Application.Interfaces;
using ChangelogSaas.Infrastructure.Auth;
using ChangelogSaas.Infrastructure.Persistence;
using ChangelogSaas.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Resend;

namespace ChangelogSaas.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            string rawConn = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            string connectionString = ToNpgsqlConnectionString(rawConn);

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(connectionString, npgsql =>
                    npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)
                          .MigrationsHistoryTable("__EFMigrationsHistory"));
                options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
            });

            services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

            services.Configure<JwtOptions>(configuration.GetSection("Jwt"));

            services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
            services.AddSingleton<ITokenService, JwtTokenService>();

            var redisConn = configuration.GetConnectionString("Redis")
                ?? throw new InvalidOperationException("Connection string 'Redis' not found.");
            services.AddStackExchangeRedisCache(opts => opts.Configuration = redisConn);
            services.AddScoped<ICacheService, RedisCacheService>();
            services.AddScoped<IBillingService, BillingService>();

            services.AddOptions();
            services.AddHttpClient<ResendClient>();
            services.Configure<ResendClientOptions>(o =>
                o.ApiToken = configuration["Resend:ApiKey"]
                    ?? throw new InvalidOperationException("Resend:ApiKey is missing."));
            services.AddTransient<IResend, ResendClient>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IBackgroundJobService, HangfireBackgroundJobService>();

            services.AddScoped<IGoogleTokenValidator, GoogleTokenValidator>();
            services.AddHttpClient<IFacebookTokenValidator, FacebookTokenValidator>();

            return services;
        }

        public static async Task MigrateDatabaseAsync(this IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.MigrateAsync();
        }

        private static string ToNpgsqlConnectionString(string cs)
        {
            if (!cs.StartsWith("postgres://") && !cs.StartsWith("postgresql://"))
                return cs;

            var uri = new Uri(cs);
            var parts = uri.UserInfo.Split(':', 2);
            var host = uri.Host;
            var port = uri.Port > 0 ? uri.Port : 5432;
            var db = uri.AbsolutePath.TrimStart('/');
            var user = parts[0];
            var pass = parts.Length > 1 ? Uri.UnescapeDataString(parts[1]) : "";

            return $"Host={host};Port={port};Database={db};Username={user};Password={pass}";
        }
    }
}
