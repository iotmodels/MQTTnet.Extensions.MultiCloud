using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud;
using MQTTnet.Extensions.MultiCloud.BrokerIoTClient;

namespace payload_size;
internal class ProtoDeviceClient
{
    internal ITelemetry<proto_model.Telemetries> Telemetry { get; set; }
    internal IReadOnlyProperty<proto_model.Properties> Props { get; set; }
    internal IReadOnlyProperty<proto_model.Properties> Prop_SdkInfo { get; set; }

    public ProtoDeviceClient(IMqttClient mqtt)
    {
        Telemetry = new TelemetryProtobuff<proto_model.Telemetries>(mqtt);
        Props = new ReadOnlyPropertyProtobuff<proto_model.Properties>(mqtt);
        Prop_SdkInfo = new ReadOnlyPropertyProtobuff<proto_model.Properties>(mqtt);
    }
}
