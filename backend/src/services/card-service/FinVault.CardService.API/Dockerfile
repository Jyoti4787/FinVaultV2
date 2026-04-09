FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/services/card-service/FinVault.CardService.API/FinVault.CardService.API.csproj", "services/card-service/FinVault.CardService.API/"]
COPY ["src/services/card-service/FinVault.CardService.Application/FinVault.CardService.Application.csproj", "services/card-service/FinVault.CardService.Application/"]
COPY ["src/services/card-service/FinVault.CardService.Domain/FinVault.CardService.Domain.csproj", "services/card-service/FinVault.CardService.Domain/"]
COPY ["src/services/card-service/FinVault.CardService.Infrastructure/FinVault.CardService.Infrastructure.csproj", "services/card-service/FinVault.CardService.Infrastructure/"]
COPY ["src/services/shared/FinVault.Shared.Contracts/FinVault.Shared.Contracts.csproj", "services/shared/FinVault.Shared.Contracts/"]
RUN dotnet restore "services/card-service/FinVault.CardService.API/FinVault.CardService.API.csproj"
COPY src/ .
RUN dotnet publish "services/card-service/FinVault.CardService.API/FinVault.CardService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FinVault.CardService.API.dll"]
