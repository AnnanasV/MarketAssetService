# MarketAssetService

> This project was completed as part of a technical assignment. Developed with a focus on real-time data handling, clean structure. A project that synchronizes and stores financial instruments and their prices from the FintaCharts platform using .NET 8, PostgreSQL, Docker, and WebSocket connections.

---

## Technology stack
- **Backend:** ASP.NET Core 8
- **Data access:** Entity Framework Core + PostgreSQL
- **Architecture:** Clean Architecture (Application / Domain / Infrastructure / API)
- **External API:** [FintaCharts](https://platform.fintacharts.com)
- **Realtime:** WebSocket subscription to prices
- **DevOps:** Docker + Docker Compose
- **Cache:** In-memory token cache
- **Testing tool:** Postman
- **Logging:** ILogger

## Getting started

### 1. Clone repository
```bash
git clone https://github.com/Annanas_V/MarketAssetService.git
cd MarketAssetService
```

### 2. Build and run in Docker
```bash
docker compose up --build
```
#### Settings
Editable in appsettings.json
``` bash
"Finta": {
    "BaseUri": "https://platform.fintacharts.com",
    "TokenUrl": "...",
    "WebSocketUrl": "wss://...{token}",
    "Username": "r_test@fintatech.com",
    "Password": "..."
  },
"ConnectionStrings": {
    "DefaultConnection": "Host=postgres;Port=5432;Database=MarketAssetDb;Username=admin;Password=admin"
  }
```

The API will be available on: http://localhost:5055
- PostgreSQL is running on port 5432 with the following credentials:
  - user: admin
  - password: admin
  - db: MarketAssetDb
 
### 3. Testing (recommended with Postman)

| Method | Route                                | Description                                  |
| ------ | ------------------------------------ | -------------------------------------------- |
| GET    | `http://localhost:5055/api/assets`               | Get all financial instruments    |
| GET    | `http://localhost:5055/api/assets/price?symbol=` | Actual asset price by symbol     |

#### Examples

<pre><code>
GET `http://localhost:5055/api/assets`
  
[
  {
        "id": "ad9e5345-4c3b-41fc-9437-1d253f62db52",
        "symbol": "AUD/CAD",
        "description": "AUD/CAD",
        "prices": [
            {
                "id": "b65da3ea-3698-42e3-b5fe-3dbbedf26f98",
                "assetId": "ad9e5345-4c3b-41fc-9437-1d253f62db52",
                "symbol": "AUD/CAD",
                "price": 0.8907,
                "timestamp": "2025-07-09T13:20:31.015313Z",
                "updatedAt": "2025-07-09T00:00:09.486564Z"
            }
        ]
    }
]
</code></pre>

<code><pre>
GET `http://localhost:5055/api/assets/price?symbol=EUR/USD`

{
    "symbol": "EUR/USD",
    "name": "EUR/USD",
    "price": 1.17155,
    "timestamp": "2025-07-09T13:22:00+00:00"
}
</code></pre>

## Docker
```bash
> market-api: .NET 8 API
> postgres: Database for storing assets and prices
```
## Project structure

<pre><code>MarketAssetService/ 
├──── Application/         Services, DTOs, Interfaces
│──── Domain/              Entities
│──── Infrastructure/      Repositories, DBcontext, external services
│──── API/                 Entry point, controllers, config 
  │──── Dockerfile         API container build file
│──── docker-compose.yml   Local dev environment
</code></pre>

## Implemented
- Database initialization via EF Core Migrations
- HTTP client for requesting financial instruments (`/api/assets`)
- WebSocket connection to FintaCharts (real-time prices)
- Access token caching via IMemoryCache
- Automatic insert/update of MarketAssets and AssetPrices
- REST API to get supported assets and latest price by symbol


