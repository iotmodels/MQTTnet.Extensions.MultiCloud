#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["pi-sense-device/pi-sense-device.csproj", "pi-sense-device/"]
RUN dotnet restore "pi-sense-device/pi-sense-device.csproj"
COPY . .
WORKDIR "/src/pi-sense-device"
RUN dotnet build "pi-sense-device.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "pi-sense-device.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "pi-sense-device.dll"]