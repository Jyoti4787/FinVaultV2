FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/services/identity-service/FinVault.IdentityService.API/FinVault.IdentityService.API.csproj", "services/identity-service/FinVault.IdentityService.API/"]
COPY ["src/services/identity-service/FinVault.IdentityService.Application/FinVault.IdentityService.Application.csproj", "services/identity-service/FinVault.IdentityService.Application/"]
COPY ["src/services/identity-service/FinVault.IdentityService.Domain/FinVault.IdentityService.Domain.csproj", "services/identity-service/FinVault.IdentityService.Domain/"]
COPY ["src/services/identity-service/FinVault.IdentityService.Infrastructure/FinVault.IdentityService.Infrastructure.csproj", "services/identity-service/FinVault.IdentityService.Infrastructure/"]
COPY ["src/services/shared/FinVault.Shared.Contracts/FinVault.Shared.Contracts.csproj", "services/shared/FinVault.Shared.Contracts/"]
RUN dotnet restore "services/identity-service/FinVault.IdentityService.API/FinVault.IdentityService.API.csproj"
COPY src/ .
RUN dotnet build "services/identity-service/FinVault.IdentityService.API/FinVault.IdentityService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "services/identity-service/FinVault.IdentityService.API/FinVault.IdentityService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FinVault.IdentityService.API.dll"]
