#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG BuildConfiguration=Release
WORKDIR /src
COPY ["NuGet.config", "."]
COPY ["Tgs.PlayerService/Tgs.PlayerService.csproj", "Tgs.PlayerService/"]
COPY ["PlayerService.Core.Cache/PlayerService.Core.Cache.csproj", "PlayerService.Core.Cache/"]
COPY ["PlayerService.Core/PlayerService.Core.csproj", "PlayerService.Core/"]
COPY ["PlayerService.Repositories/PlayerService.Repositories.csproj", "PlayerService.Repositories/"]
RUN dotnet restore "Tgs.PlayerService/Tgs.PlayerService.csproj"
COPY . .
WORKDIR "/src/Tgs.PlayerService"
RUN dotnet build "Tgs.PlayerService.csproj" -c $BuildConfiguration -o /app/build

FROM build AS publish
ARG BuildConfiguration
RUN dotnet publish "Tgs.PlayerService.csproj" -c $BuildConfiguration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tgs.PlayerService.dll"]