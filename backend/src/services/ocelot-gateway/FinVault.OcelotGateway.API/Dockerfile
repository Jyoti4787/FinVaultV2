FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/services/ocelot-gateway/FinVault.OcelotGateway.API/FinVault.OcelotGateway.API.csproj", "services/ocelot-gateway/FinVault.OcelotGateway.API/"]
RUN dotnet restore "services/ocelot-gateway/FinVault.OcelotGateway.API/FinVault.OcelotGateway.API.csproj"
COPY src/ .
RUN dotnet build "services/ocelot-gateway/FinVault.OcelotGateway.API/FinVault.OcelotGateway.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "services/ocelot-gateway/FinVault.OcelotGateway.API/FinVault.OcelotGateway.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=build /src/services/ocelot-gateway/FinVault.OcelotGateway.API/ocelot.json .
ENTRYPOINT ["dotnet", "FinVault.OcelotGateway.API.dll"]
