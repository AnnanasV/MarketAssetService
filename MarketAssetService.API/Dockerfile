FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5050

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY MarketAssetService.API/*.csproj MarketAssetService.API/
COPY MarketAssetService.Infrastructure/*.csproj MarketAssetService.Infrastructure/
COPY MarketAssetService.Domain/*.csproj MarketAssetService.Domain/

RUN dotnet restore MarketAssetService.API/MarketAssetService.API.csproj

COPY . .

WORKDIR /src/MarketAssetService.API
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "MarketAssetService.API.dll"]