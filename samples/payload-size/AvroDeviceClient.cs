using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud;
using MQTTnet.Extensions.MultiCloud.BrokerIoTClient;
using payload_size.Binders;

namespace payload_size;

internal class AvroDeviceClient
{
    internal ITelemetry<avros.Telemetries> Telemetry { get; set; }
    internal IReadOnlyProperty<avros.Properties> Props { get; set; }
    internal IReadOnlyProperty<avros.Properties> Prop_SdkInfo { get; set; }

    public AvroDeviceClient(IMqttClient mqtt)
    {
        Telemetry = new TelemetryAvro<avros.Telemetries>(mqtt, new avros.Telemetries().Schema);
        Props = new ReadOnlyPropertyAvro<avros.Properties>(mqtt, new avros.Properties().Schema);
        Prop_SdkInfo = new ReadOnlyPropertyAvro<avros.Properties>(mqtt, new avros.Properties().Schema);
    }
}
