FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["FreeSpinsGame/FreeSpinsGame.csproj", "FreeSpinsGame/"]
RUN dotnet restore "FreeSpinsGame/FreeSpinsGame.csproj"
COPY . .
WORKDIR "/src/FreeSpinsGame"
RUN dotnet build "./FreeSpinsGame.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "./FreeSpinsGame.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FreeSpinsGame.dll"]
