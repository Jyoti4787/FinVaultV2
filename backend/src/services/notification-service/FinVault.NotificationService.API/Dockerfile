FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/services/notification-service/FinVault.NotificationService.API/FinVault.NotificationService.API.csproj", "services/notification-service/FinVault.NotificationService.API/"]
COPY ["src/services/notification-service/FinVault.NotificationService.Application/FinVault.NotificationService.Application.csproj", "services/notification-service/FinVault.NotificationService.Application/"]
COPY ["src/services/notification-service/FinVault.NotificationService.Domain/FinVault.NotificationService.Domain.csproj", "services/notification-service/FinVault.NotificationService.Domain/"]
COPY ["src/services/notification-service/FinVault.NotificationService.Infrastructure/FinVault.NotificationService.Infrastructure.csproj", "services/notification-service/FinVault.NotificationService.Infrastructure/"]
COPY ["src/services/shared/FinVault.Shared.Contracts/FinVault.Shared.Contracts.csproj", "services/shared/FinVault.Shared.Contracts/"]
RUN dotnet restore "services/notification-service/FinVault.NotificationService.API/FinVault.NotificationService.API.csproj"
COPY src/ .
RUN dotnet build "services/notification-service/FinVault.NotificationService.API/FinVault.NotificationService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "services/notification-service/FinVault.NotificationService.API/FinVault.NotificationService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FinVault.NotificationService.API.dll"]
