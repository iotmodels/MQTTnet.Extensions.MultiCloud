using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.BrokerIoTClient;

namespace payload_size
{
    internal class ProtoDeviceClient
    {
        internal TelemetryProtobuff<proto_model.Telemetries> Telemetry { get; set; }
        internal ReadOnlyPropertyProtobuff<proto_model.Properties> Props { get; set; }

        public ProtoDeviceClient(IMqttClient mqtt)
        {
            Telemetry = new TelemetryProtobuff<proto_model.Telemetries>(mqtt);
            Props = new ReadOnlyPropertyProtobuff<proto_model.Properties>(mqtt);
        }
    }
}
