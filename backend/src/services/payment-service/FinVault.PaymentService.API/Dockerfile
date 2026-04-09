FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/services/payment-service/FinVault.PaymentService.API/FinVault.PaymentService.API.csproj", "services/payment-service/FinVault.PaymentService.API/"]
COPY ["src/services/payment-service/FinVault.PaymentService.Application/FinVault.PaymentService.Application.csproj", "services/payment-service/FinVault.PaymentService.Application/"]
COPY ["src/services/payment-service/FinVault.PaymentService.Domain/FinVault.PaymentService.Domain.csproj", "services/payment-service/FinVault.PaymentService.Domain/"]
COPY ["src/services/payment-service/FinVault.PaymentService.Infrastructure/FinVault.PaymentService.Infrastructure.csproj", "services/payment-service/FinVault.PaymentService.Infrastructure/"]
COPY ["src/services/shared/FinVault.Shared.Contracts/FinVault.Shared.Contracts.csproj", "services/shared/FinVault.Shared.Contracts/"]
RUN dotnet restore "services/payment-service/FinVault.PaymentService.API/FinVault.PaymentService.API.csproj"
COPY src/ .
RUN dotnet publish "services/payment-service/FinVault.PaymentService.API/FinVault.PaymentService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FinVault.PaymentService.API.dll"]
