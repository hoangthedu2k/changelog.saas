using System.Text;
using System.Text.Json.Serialization;
using ChangelogSaas.API.Endpoints;
using ChangelogSaas.API.Middleware;
using ChangelogSaas.Application;
using ChangelogSaas.Infrastructure;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseSentry(o =>
{
    o.Dsn = builder.Configuration["Sentry:Dsn"] ?? "";
    o.TracesSampleRate = double.TryParse(builder.Configuration["Sentry:TracesSampleRate"], out var rate) ? rate : 0.2;
    o.Environment = builder.Configuration["Sentry:Environment"] ?? builder.Environment.EnvironmentName;
    o.SendDefaultPii = false;
});

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                     .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                     .AddJsonFile("appsettings.Secrets.json", optional: true, reloadOnChange: true)
                     .AddEnvironmentVariables();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var pgConn = ToNpgsqlConnectionString(
    builder.Configuration.GetConnectionString("DefaultConnection")!);
builder.Services.AddHangfire(cfg => cfg
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(opts => opts.UseNpgsqlConnection(pgConn)));
builder.Services.AddHangfireServer();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddOpenApi();

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

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:4200"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod());

    options.AddPolicy("Widget", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .WithMethods("GET"));
});

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard("/hangfire");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("ChangelogSaaS API");
        options.WithDefaultHttpClient(ScalarTarget.Http, ScalarClient.HttpClient);
    });
}

app.MapGet("/api/health", () => Results.Ok(new { status = "ok", timestamp = DateTime.UtcNow }));

app.MapAuthEndpoints();
app.MapProjectEndpoints();
app.MapEntryEndpoints();
app.MapWidgetEndpoints();
app.MapSubscriberEndpoints();

await app.Services.MigrateDatabaseAsync();

app.Run();

// Converts postgres:// or postgresql:// URL to Npgsql key-value format.
// Hangfire.PostgreSql does not accept URL-format connection strings.
static string ToNpgsqlConnectionString(string cs)
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
