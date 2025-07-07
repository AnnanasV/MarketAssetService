FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5055

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MarketAssetService.API/MarketAssetService.API.csproj", "MarketAssetService.API/"]
COPY ["MarketAssetService.Application/MarketAssetService.Application.csproj", "MarketAssetService.Application/"]
COPY ["MarketAssetService.Domain/MarketAssetService.Domain.csproj", "MarketAssetService.Domain/"]
COPY ["MarketAssetService.Infrastructure/MarketAssetService.Infrastructure.csproj", "MarketAssetService.Infrastructure/"]
RUN dotnet restore "MarketAssetService.API/MarketAssetService.API.csproj"
COPY . .
WORKDIR "/src/MarketAssetService.API"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY MarketAssetService.API/appsettings.json ./
ENTRYPOINT ["dotnet", "MarketAssetService.API.dll"]
