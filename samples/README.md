# MQTTnet.Exntensions.MultiCloud Samples

This folder contains sample projects with devices that can be connected to multiple MQTT endpoints

>Note: All samples must be run with the `ConnectionStrings__cs` envvar, or as CLI argument `/ConnectionStrings:cs` . There are sample connection strings in the launchSettings.json.template to use as `dotnet run --launch-profile`

## memmon

The legendary Memory Monitor implemented for Broker, IoTHub and AWS.

Docker image available as `ghcr.io/iotmodes/memmon:x64`

## pi-sense-device

Using the sensors available from the PiSenseHat, implementing the model https://iotmodels.github.io/dmr/dtmi/com/example/devicetemplate-1.json

Docker image avaialble as ` ghcr.io/iotmodels/pi-sense-device:x64`

## mqtt-device

Implements a sample `DeviceTemplate` model, targets Hub and Broker

## iothub-sample

Shows how to use the `AzureIoTClient` to connect to Azure IoT Hub and DPS using `HubMqttClient` (untyped APIs)