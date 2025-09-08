# Use the official .NET 8 runtime as base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000
EXPOSE 5001

# Use the .NET 8 SDK for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["UpgradeNotificationSystem.csproj", "."]
RUN dotnet restore "UpgradeNotificationSystem.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "UpgradeNotificationSystem.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "UpgradeNotificationSystem.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UpgradeNotificationSystem.dll"]