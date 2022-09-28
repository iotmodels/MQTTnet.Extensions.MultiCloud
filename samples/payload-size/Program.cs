
using Google.Protobuf.WellKnownTypes;
using MQTTnet.Extensions.MultiCloud.BrokerIoTClient;
using MQTTnet.Extensions.MultiCloud.Connections;
using System.Diagnostics;
using payload_size;

using var ctl = new ConsoleTraceListener();
Trace.Listeners.Add(ctl);

var mqtt = await BrokerClientFactory.CreateFromConnectionSettingsAsync(new ConnectionSettings()
{
    HostName = "localhost",
    TcpPort = 1883,
    UseTls = false,
    ClientId = "payloadSize",
    UserName = "user",
    Password = "password"
}, false);

var jsonClient = new JsonDeviceClient(mqtt);
await jsonClient.Telemetry.SendMessageAsync(
    new JsonDeviceClient.Telemetries { 
        Temperature = 23.211, 
        WorkingSet = Environment.WorkingSet
    });

await jsonClient.Props.SendMessageAsync(
    new JsonDeviceClient.Properties { 
        SdkInfo = BrokerClientFactory.NuGetPackageVersion, 
        Started = DateTime.UtcNow
    });

var protoClient = new ProtoDeviceClient(mqtt);
await protoClient.Telemetry.SendMessageAsync(
    new proto_model.Telemetries { 
        Temperature = 23.321, 
        WorkingSet = Environment.WorkingSet 
    });

await protoClient.Props.SendMessageAsync(
    new proto_model.Properties
    {
        SdkInfo = BrokerClientFactory.NuGetPackageVersion,
        Started = DateTime.UtcNow.ToTimestamp()
    });

