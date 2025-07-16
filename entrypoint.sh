#!/bin/bash
set -e

# Run migrations
export PATH="$PATH:/root/.dotnet/tools"
dotnet ef database update --project /app/portfolioAPI.csproj

# Start the app
exec dotnet portfolioAPI.dll 