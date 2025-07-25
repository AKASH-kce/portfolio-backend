# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY portfolioAPI/portfolioAPI.csproj ./portfolioAPI/
RUN dotnet restore ./portfolioAPI/portfolioAPI.csproj

# Install EF Core CLI tools
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"

# Copy the rest of the code
COPY portfolioAPI/. ./portfolioAPI/

WORKDIR /src/portfolioAPI
RUN dotnet publish -c Release -o /app/out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./
COPY entrypoint.sh ./
RUN chmod +x ./entrypoint.sh
ENTRYPOINT ["./entrypoint.sh"] 