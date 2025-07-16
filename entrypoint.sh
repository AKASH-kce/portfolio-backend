#!/bin/bash
set -ex

# Run migrations
export PATH="$PATH:/root/.dotnet/tools"

echo "Running migrations..."
dotnet ef database update --project /app/portfolioAPI.csproj

echo "Starting app..."
exec dotnet portfolioAPI.dll 