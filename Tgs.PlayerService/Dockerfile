#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BuildConfiguration=Release
RUN --mount=type=secret,id=nuget,dst=/pat cp /pat ~/.nuget/NuGet/NuGet.Config
WORKDIR /src
COPY ["NuGet.config", "."]
COPY ["Tgs.PlayerService/Tgs.PlayerService.csproj", "Tgs.PlayerService/"]
COPY ["PlayerService.Repositories/PlayerService.Repositories.csproj", "PlayerService.Repositories/"]
COPY ["PlayerService.Core/PlayerService.Core.csproj", "PlayerService.Core/"]
RUN dotnet restore "Tgs.PlayerService/Tgs.PlayerService.csproj"
COPY . .
WORKDIR "/src/Tgs.PlayerService"
RUN dotnet build "Tgs.PlayerService.csproj" -c $BuildConfiguration -o /app/build

FROM build AS publish
ARG BuildConfiguration
RUN dotnet publish "Tgs.PlayerService.csproj" -c $BuildConfiguration -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tgs.PlayerService.dll"]