#!/bin/bash
echo "Waiting for PostgreSQL to be ready..."
until pg_isready -h postgres -p 5432 -U admin; do
  sleep 1
done

echo "PostgreSQL is ready. Launching MarketAssetService.API..."
dotnet MarketAssetService.API.dll