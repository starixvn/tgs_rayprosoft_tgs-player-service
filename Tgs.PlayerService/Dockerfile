#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM hub.lumigame.com/base/dotnet/core/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["NuGet.config", "."]
COPY ["Tgs.PlayerService/Tgs.PlayerService.csproj", "Tgs.PlayerService/"]
COPY ["PlayerService.Core.Cache/PlayerService.Core.Cache.csproj", "PlayerService.Core.Cache/"]
COPY ["PlayerService.Core/PlayerService.Core.csproj", "PlayerService.Core/"]
COPY ["PlayerService.Repositories/PlayerService.Repositories.csproj", "PlayerService.Repositories/"]
RUN dotnet restore "./Tgs.PlayerService/./Tgs.PlayerService.csproj"
COPY . .
WORKDIR "/src/Tgs.PlayerService"
RUN dotnet build "./Tgs.PlayerService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Tgs.PlayerService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM hub.lumigame.com/base/dotnet/core/aspnet:8.0-alpine-24h1.1 AS base
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tgs.PlayerService.dll"]