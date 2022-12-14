#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/nightly/runtime:7.0-jammy-chiseled AS base
WORKDIR /app

FROM  mcr.microsoft.com/dotnet/sdk:7.0-jammy  AS build
WORKDIR /src
COPY ["samples/mqtt-grpc-device/mqtt-grpc-device.csproj", "samples/mqtt-grpc-device/"]
COPY ["src/MQTTnet.Extensions.MultiCloud.BrokerIoTClient/MQTTnet.Extensions.MultiCloud.BrokerIoTClient.csproj", "src/MQTTnet.Extensions.MultiCloud.BrokerIoTClient/"]
COPY ["src/MQTTnet.Extensions.MultiCloud/MQTTnet.Extensions.MultiCloud.csproj", "src/MQTTnet.Extensions.MultiCloud/"]
RUN dotnet restore "samples/mqtt-grpc-device/mqtt-grpc-device.csproj"
COPY . .
WORKDIR "/src/samples/mqtt-grpc-device"
RUN dotnet build "mqtt-grpc-device.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "mqtt-grpc-device.csproj" -c Release -o /app/publish --os linux --arch x64 --use-current-runtime --self-contained false --no-restore 

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "mqtt-grpc-device.dll"]