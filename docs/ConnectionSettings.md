# Connection Settings

Connection settings can be established using the API or parsing a connection string with the next options:

- `HostName` MQTT host name
- `IdScope` DPS IdScope 
- `DeviceId` Device Identity 
- `SharedAccessKey` Device Shared Access Key
- `SasMinutes` SasToken expire time in minutes, default to `60`.
- `X509Key` __pathtopfx>|<pfxpassword__  see details in [X509Certificates](X509Certificates.md)
- `ModelId` DTDL Model ID in DTMI/Proto format to indicate the model the device implements
- `ModuleId` IoTHub Device Module Identity
- `UserName` Username to be used to authenticate with MQTT Brokers
- `Password` Username to be used to authenticate with MQTT Brokers
- `ClientId` Client ID used when connecting to MQTT Brokers (IoT Hub requires used deviceId as clientId). Use `{machineName}` to override
- `KeepAliveInSeconds` Seconds to send keep alive packets, default to `60`
- `CleanSession` Establish the connection with a clean session, default to `true`
- `TcpPort` Sets the TCP port for the MQTT connection, defaults to `8883`
- `UseTls` Enable/Disable Server TLS connection, defaults to `true`
- `CaFile` Path to the CA certificate required to stablish the TLS session

## Sample Connection Strings

Azure IoT Hub

```
HostName=<hubName>.azure-devices.net;DeviceId=<deviceId>;SharedAccessKey=<deviceSasKey>
```

Azure Device Provisioning Service/Azure IoT Central

```
IdScope=<dps-id-scope>;DeviceId=<deviceId>;SharedAccessKey=<deviceSasKey>
```

Azure IoT Plug and Play

```
HostName=<hubName>.azure-devices.net;DeviceId=<deviceId>;SharedAccessKey=<deviceSasKey>;ModelId=<dtmi:yourmodel;1>

IdScope=<dps-id-scope>;DeviceId=<deviceId>;SharedAccessKey=<deviceSasKey>;ModelId=<dtmi:yourmodel;1>
```

Using Certificates

```
HostName=<hubName>.azure-devices.net;X509Key=<path-to-pfx>|<pfx-password>

IdScope=<dps-id-scope>;X509Key=<path-to-pfx>|<pfx-password>
```

Advanced Connection Options

```
HostName=<hubName>.azure-devices.net;DeviceId=<deviceId>;SharedAccessKey=<deviceSasKey>;ModelId=<dtmi:yourmodel;1>;SasMinutes=60;KeepAliveInSeconds=30

IdScope=<dps-id-scope>;DeviceId=<deviceId>;SharedAccessKey=<deviceSasKey>;ModelId=<dtmi:yourmodel;1>;SasMinutes=60;KeepAliveInSeconds=30
```

Connecting to a MQTT Broker

```
HostName=<broker-hostname>;UserName=<user-name>;Password=<password>;CliendId=<client-id>

HostName=<broker-hostname>;X509Key=<path-to-pfx>|<pfx-password>;ClientId=<client-id>
```


> Note: All samples use the connection settings in the `ConnectionString` configuration, available in the `appSettings.json` file, or as the environment variable `ConnectionStrings__cs`, or command line argument `/ConnnectionStrings:cs`