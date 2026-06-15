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
            string connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

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
    }
}
