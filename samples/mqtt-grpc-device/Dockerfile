#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.


FROM  mcr.microsoft.com/dotnet/sdk:7.0-jammy  AS build
WORKDIR /samples
COPY "samples/mqtt-grpc-device/." .

RUN dotnet remove reference ../../src/MQTTnet.Extensions.MultiCloud.BrokerIoTClient/MQTTnet.Extensions.MultiCloud.BrokerIoTClient.csproj
RUN dotnet add package MQTTnet.Extensions.MultiCloud.BrokerIoTClient --prerelease -n

RUN dotnet restore "mqtt-grpc-device.csproj" --use-current-runtime
RUN dotnet build "mqtt-grpc-device.csproj" -c Release --use-current-runtime
RUN dotnet publish "mqtt-grpc-device.csproj" -c Release -o /app  --use-current-runtime --self-contained false --no-restore 

FROM mcr.microsoft.com/dotnet/nightly/runtime:7.0-jammy-chiseled
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "mqtt-grpc-device.dll"]