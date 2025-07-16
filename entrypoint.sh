#!/bin/bash
set -e

# Run migrations
export PATH="$PATH:/root/.dotnet/tools"
dotnet ef database update

# Start the app
exec dotnet portfolioAPI.dll 