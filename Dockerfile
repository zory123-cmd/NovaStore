# =============================================================================
# NovaStore.WebApi — Multi-stage Dockerfile for .NET 9
# =============================================================================

# ---- Build Stage ----
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /build

# Copy project files for layer caching
COPY src/NovaStore.Domain/*.csproj    src/NovaStore.Domain/
COPY src/NovaStore.Application/*.csproj  src/NovaStore.Application/
COPY src/NovaStore.WebApi/*.csproj    src/NovaStore.WebApi/

# Restore NuGet packages
RUN dotnet restore src/NovaStore.WebApi/NovaStore.WebApi.csproj

# Copy the full source code
COPY src/ src/

# Build and publish
RUN dotnet publish src/NovaStore.WebApi/NovaStore.WebApi.csproj \
    -c Release \
    -o /app/publish

# ---- Runtime Stage ----
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "NovaStore.WebApi.dll"]
