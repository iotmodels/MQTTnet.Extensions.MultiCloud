using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud;
using payload_size.Binders;

namespace payload_size;
internal class ProtoDeviceClient
{
    internal ITelemetry<proto_model.Telemetries> Telemetry { get; set; }
    internal IReadOnlyProperty<proto_model.Properties> Props { get; set; }
    internal IReadOnlyProperty<proto_model.Properties> Prop_SdkInfo { get; set; }

    public ProtoDeviceClient(IMqttClient mqtt)
    {
        Telemetry = new TelemetryProtobuf<proto_model.Telemetries>(mqtt);
        Props = new ReadOnlyPropertyProtobuff<proto_model.Properties>(mqtt);
        Prop_SdkInfo = new ReadOnlyPropertyProtobuff<proto_model.Properties>(mqtt);
    }
}
