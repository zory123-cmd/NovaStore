# =============================================================================
# NovaStore.WebApi — Multi-stage Dockerfile for .NET 9
# =============================================================================

# ---- Build Stage ----
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# 1️⃣ Copy project files (layer cached until a .csproj changes)
COPY src/NovaStore.Domain/NovaStore.Domain.csproj    src/NovaStore.Domain/
COPY src/NovaStore.Application/NovaStore.Application.csproj  src/NovaStore.Application/
COPY src/NovaStore.WebApi/NovaStore.WebApi.csproj    src/NovaStore.WebApi/

# 2️⃣ Restore NuGet packages
RUN dotnet restore src/NovaStore.WebApi/NovaStore.WebApi.csproj

# 3️⃣ Copy the remaining source code and publish
COPY src/ src/
WORKDIR /src/NovaStore.WebApi
RUN dotnet publish -c Release -o /app/publish \
    --no-restore

# ---- Runtime Stage ----
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# ASP.NET Core Kestrel listens on port 8080 by default in the container
EXPOSE 8080

# Copy the published output from the build stage
COPY --from=build /app/publish .

# Health check (optional — uncomment if you add health-check endpoints)
# HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
#   CMD curl --fail http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "NovaStore.WebApi.dll"]
