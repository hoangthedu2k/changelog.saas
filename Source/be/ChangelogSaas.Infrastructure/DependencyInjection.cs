using ChangelogSaas.Application.Interfaces;
using ChangelogSaas.Infrastructure.Auth;
using ChangelogSaas.Infrastructure.Persistence;
using ChangelogSaas.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

            return services;
        }
    }
}
