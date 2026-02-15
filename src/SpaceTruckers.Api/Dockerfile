# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props NuGet.config SpaceTruckers.sln ./

# Copy project files first for better layer caching
COPY src/SpaceTruckers.Domain/SpaceTruckers.Domain.csproj src/SpaceTruckers.Domain/
COPY src/SpaceTruckers.Application/SpaceTruckers.Application.csproj src/SpaceTruckers.Application/
COPY src/SpaceTruckers.Infrastructure/SpaceTruckers.Infrastructure.csproj src/SpaceTruckers.Infrastructure/
COPY src/SpaceTruckers.Api/SpaceTruckers.Api.csproj src/SpaceTruckers.Api/

RUN dotnet restore src/SpaceTruckers.Api/SpaceTruckers.Api.csproj

# Copy the rest of the source
COPY src/ src/

RUN dotnet publish src/SpaceTruckers.Api/SpaceTruckers.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish ./

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "SpaceTruckers.Api.dll"]
