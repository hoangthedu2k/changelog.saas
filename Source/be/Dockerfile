# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ChangelogSaaS.slnx ./
COPY ChangelogSaas.API/ChangelogSaas.API.csproj ChangelogSaas.API/
COPY ChangelogSaas.Application/ChangelogSaas.Application.csproj ChangelogSaas.Application/
COPY ChangelogSaas.Domain/ChangelogSaas.Domain.csproj ChangelogSaas.Domain/
COPY ChangelogSaas.Infrastructure/ChangelogSaas.Infrastructure.csproj ChangelogSaas.Infrastructure/
COPY ChangelogSaas.Tests/ChangelogSaas.Tests.csproj ChangelogSaas.Tests/

RUN dotnet restore

COPY . .

RUN dotnet publish ChangelogSaas.API/ChangelogSaas.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

# Use shell form so Railway's $PORT env var is substituted at runtime
CMD ["sh", "-c", "ASPNETCORE_HTTP_PORTS=${PORT:-8080} dotnet ChangelogSaas.API.dll"]
