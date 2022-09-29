
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

Trace.WriteLine("JSON");
var jsonClient = new JsonDeviceClient(mqtt);
await jsonClient.Telemetry.SendMessageAsync(
    new payload_size.json.Telemetries
    { 
        Temperature = 23.211, 
        WorkingSet = Environment.WorkingSet
    });

await jsonClient.Props.SendMessageAsync(
    new payload_size.json.Properties
    { 
        SdkInfo = BrokerClientFactory.NuGetPackageVersion, 
        Started = DateTime.UtcNow
    });

await jsonClient.Prop_SdkInfo.SendMessageAsync(BrokerClientFactory.NuGetPackageVersion);
Trace.WriteLine("");

Trace.WriteLine("Proto");
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

await protoClient.Prop_SdkInfo.SendMessageAsync(
    new proto_model.Properties
    {
        SdkInfo = BrokerClientFactory.NuGetPackageVersion,
    });
Trace.WriteLine("");

Trace.WriteLine("Avro");
var avroClient = new AvroDeviceClient(mqtt);
await avroClient.Telemetry.SendMessageAsync(
    new payload_size.avros.Telemetries
    {
        Temperature = 23.321,
        WorkingSet = Environment.WorkingSet
    });
await avroClient.Props.SendMessageAsync(
    new payload_size.avros.Properties
    {
        SdkInfo = BrokerClientFactory.NuGetPackageVersion,
        Started = DateTime.UtcNow.ToBinary()
    });


